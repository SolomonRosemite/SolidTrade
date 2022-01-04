using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger _logger = Log.ForContext<UserService>();
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;
        
        public UserService(DbSolidTrade database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<UserResponseDto, ErrorResponse>> CreateUser(CreateUserRequestDto data, string uid)
        {
            var user = new User
            {
                Email = data.Email,
                Uid = uid,
                Username = data.Username,
                DisplayName = data.DisplayName,
                Portfolio = new Portfolio
                {
                    Balance = 10000,
                },
                HistoricalPositions = new List<HistoricalPosition>(),
                HasPublicPortfolio = true,
            };

            var usernameTaken = await AsyncEnumerable.AnyAsync(_database.Users, u => u.Username == user.Username);
            var userUidAlreadyInUse = await AsyncEnumerable.AnyAsync(_database.Users, u => u.Uid == user.Uid);

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
            
            return _mapper.Map<UserResponseDto>(newUser.Entity);
        }
        
        public async Task<OneOf<UserResponseDto, ErrorResponse>> GetUserById(int id, string uid)
        {
            var user = await _database.Users.FindAsync(id);

            if (user is null)
            {
                return new ErrorResponse(new UserNotFound
                {
                    Title = "User not found",
                    Message = $"The user with id: {id} could not be found.",
                }, HttpStatusCode.NotFound);
            }
            
            var userResponse = _mapper.Map<UserResponseDto>(user);

            // If request user is not owner of user hide private information.
            if (user.Uid != uid)
                userResponse.Email = null;

            return userResponse;
        }
        
        public async Task<OneOf<List<UserResponseDto>, ErrorResponse>> SearchUserByUsername(string username)
        {
            var users = await _database.Users.AsQueryable()
                .Where(u =>  EF.Functions.Like(u.Username, $"{username}%"))
                .ToListAsync();

            return users.Select(user =>
            {
                user.Email = null;
                return _mapper.Map<UserResponseDto>(user);
            }).ToList();
        }
        
        public async Task<OneOf<UserResponseDto, ErrorResponse>> UpdateUser(UpdateUserDto dto, string uid)
        {
            if (dto.Username?.Length < 3 || dto.DisplayName?.Length < 3)
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Username or DisplayName too short",
                    Message = "The DisplayName or and Username must at least be 3 characters long.",
                }, HttpStatusCode.BadRequest);
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
            
            if (dto.Username is not null && await AsyncEnumerable.AnyAsync(_database.Users, u => u.Username == dto.Username))
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Username taken",
                    Message = $"The username: {dto.Username} is already in use",
                    UserFriendlyMessage = "The username is unfortunately already in use. Please choose another one.",
                }, HttpStatusCode.Conflict);
            }

            if (dto.Email is not null && await AsyncEnumerable.AnyAsync(_database.Users, u => u.Email == dto.Email))
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
                return _mapper.Map<UserResponseDto>(user);
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

        public async Task<OneOf<DeleteUserResponseDto, ErrorResponse>> DeleteUser(string uid)
        {
            var user = await AsyncEnumerable.FirstOrDefaultAsync(_database.Users, u => u.Uid == uid);

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

            try
            {
                _database.Users.Remove(user);
                await _database.SaveChangesAsync();
                _logger.Information($"User delete with id: {user.Id} was successful");
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UserDeleteFailed
                {
                    Title = "Could not delete user",
                    Message = $"Could not delete user with id: {user.Id} from the database.",
                    Exception = e,
                }, HttpStatusCode.InternalServerError);
            }
            
            return new DeleteUserResponseDto { Successful = true };
        }
    }
}