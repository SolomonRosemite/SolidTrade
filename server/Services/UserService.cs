using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Data.Dtos.User.Response;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Common;

namespace SolidTradeServer.Services
{
    public class UserService
    {
        private readonly AuthenticationService _authService;
        private readonly ILogger _logger;
        private readonly DbSolidTrade _database;
        
        public UserService(DbSolidTrade database, AuthenticationService authService, ILogger logger)
        {
            _database = database;
            _authService = authService;
            _logger = logger;
        }

        public async Task<OneOf<User, ErrorResponse>> CreateUser(CreateUserRequestDto data)
        {
            var user = new User
            {
                Email = data.Email,
                Uid = data.Uid,
                Username = data.Username,
                DisplayName = data.DisplayName,
                Portfolio = new Portfolio(),
                HistoricalPositions = new List<HistoricalPosition>(),
                HasPublicPortfolio = true,
            };

            var usernameTaken = await _database.Users.AnyAsync(u => u.Username == user.Username);
            var userUidAlreadyInUse = await _database.Users.AnyAsync(u => u.Uid == user.Uid);

            if (usernameTaken || userUidAlreadyInUse)
            {
                return new ErrorResponse(new UserCreateFailed
                {
                    Title = usernameTaken ? "Username taken" : "Uid already in use",
                    Message = usernameTaken ? "The username is already in use." : "Can not create user with existing uid.",
                    UserFriendlyMessage = usernameTaken
                        ? "The username is unfortunately already in use. Please choose another one."
                        : "Seems like this google account is already linked to another user. Please choose another google account.",
                }, HttpStatusCode.Conflict);
            }

            EntityEntry<User> newUser;
            
            try
            {
                newUser = await _database.Users.AddAsync(user);
                await _database.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Failed to create new user",
                    Message = "Failed to create new user.",
                    UserFriendlyMessage = "Something went wrong. Please try again later.",
                    Exception = e,
                }, HttpStatusCode.InternalServerError);
            }

            return newUser.Entity;
        }
        
        public async Task<OneOf<User, ErrorResponse>> GetUserById(int id)
        {
            var user = await _database.Users.FindAsync(id);

            if (user is not null)
                return user;
            
            return new ErrorResponse(new UserNotFound
            {
                Title = "User not found",
                Message = $"The user with id: {id} could not be found.",
            }, HttpStatusCode.NotFound);
        }
        
        public async Task<OneOf<IEnumerable<User>, ErrorResponse>> GetUserByUsername(string username)
        {
            // Todo: Check if this is case insensitive.
            var users = await _database.Users.AsAsyncEnumerable()
                .Where(u => u.Username.Contains(username))
                .ToListAsync();

            return users;
        }
        
        public async Task<OneOf<User, ErrorResponse>> UpdateUser(UpdateUserDto dto)
        {
            if (dto.Username?.Length < 3 || dto.DisplayName?.Length < 3)
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Username or DisplayName too short",
                    Message = "The DisplayName or and Username must at least be 3 characters long.",
                }, HttpStatusCode.BadRequest);
            }
            
            var (successful, uid) = await _authService.AuthenticateUser(dto.Token);

            if (!successful)
            {
                return new ErrorResponse(new NotAuthenticated
                {
                    Title = "Invalid token",
                    Message = "The token provided is expired or invalid.",
                }, HttpStatusCode.Unauthorized);
            }
            
            var user = await _database.Users.FindAsync(dto.Id);

            if (user is null)
                return new ErrorResponse(new UserNotFound
                {
                    Title = "User not found",
                    Message = $"User with id: {dto.Id} could not be found.",
                }, HttpStatusCode.NotFound);
            
            if (user.Uid != uid)
            {
                return new ErrorResponse(new NotAuthorized
                {
                    Title = "Tried updating another user",
                    Message = "The target user to update is not the same as the user requesting to update.",
                }, HttpStatusCode.Unauthorized);
            }
            
            if (dto.Username is not null && await _database.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Username taken",
                    Message = $"The username: {dto.Username} is already in use",
                    UserFriendlyMessage = "The username is unfortunately already in use. Please choose another one.",
                }, HttpStatusCode.Conflict);
            }

            if (dto.Email is not null && await _database.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Email already in use",
                    Message = $"The email: {dto.Email} is already in use.",
                    UserFriendlyMessage = "Seems like this google account is already linked to another user. Please choose another google account.",
                }, HttpStatusCode.Conflict);
            }
            
            user.Email = dto.Email ?? user.Email;
            user.Username = dto.Username ?? user.Username;
            user.DisplayName = dto.DisplayName ?? user.DisplayName;
            user.ProfilePictureUrl = dto.ProfilePictureUrl ?? user.ProfilePictureUrl;

            try
            {
                _database.Users.Update(user);
                await _database.SaveChangesAsync();
                return user;
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Failed to save updated user",
                    Message = $"Failed to update user with id: {dto.Id}",
                    Exception = e,
                }, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<OneOf<DeleteUserResponseDto, ErrorResponse>> DeleteUser(DeleteUserRequestDto dto)
        {
            var (isAuthenticated, uid) = await _authService.AuthenticateUser(dto.Token);

            if (!isAuthenticated)
            {
                return new ErrorResponse(new NotAuthenticated
                {
                    Title = "Authentication failed",
                    Message = "User token invalid",
                    UserFriendlyMessage = "User token invalid. Please try logging out then in again.",
                }, HttpStatusCode.BadRequest);
            }
            
            var user = await _database.Users.FirstOrDefaultAsync(u => u.Uid == uid);

            if (user is null)
            {
                return new ErrorResponse(new UserNotFound
                {
                    Title = "User not found",
                    Message = $"Could not find user with uid: {uid}",
                }, HttpStatusCode.InternalServerError);
            }
            
            try
            {
                await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid, CancellationToken.None);
                _logger.Information($"Firebase user delete with id: {uid} was successful");
            }
            catch (Exception e)
            {
                var error = new ErrorResponse(new UserDeleteFailed
                {
                    Title = "Could not delete user",
                    Message = $"Delete user with uid: {uid} failed.",
                    Exception = e,
                }, HttpStatusCode.InternalServerError);

                return error;
            }
            
            await CommonService.Firestore
                .Document($"users/{uid}")
                .DeleteAsync();

            _database.Users.Remove(user);
            _database.SaveChanges();
            _logger.Information($"User delete with id: {user.Id} was successful");

            return new DeleteUserResponseDto { Successful = true };
        }
    }
}