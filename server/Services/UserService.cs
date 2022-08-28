using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Data.Dtos.User.Response;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using NotFound = SolidTradeServer.Data.Models.Errors.NotFound;

namespace SolidTradeServer.Services
{
    public class UserService
    {
        private readonly ILogger _logger = Log.ForContext<UserService>();
        
        private readonly CloudinaryService _cloudinaryService;
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;
        
        public UserService(DbSolidTrade database, IMapper mapper, CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<UserResponseDto, ErrorResponse>> CreateUser(CreateUserRequestDto dto, string uid)
        {
            var user = new User
            {
                Bio = $"Hi, I'm {dto.DisplayName} and this is my Portfolio. 👋",
                Email = dto.Email,
                Uid = uid,
                Username = dto.Username,
                DisplayName = dto.DisplayName,
                Portfolio = new Portfolio
                {
                    Cash = dto.InitialBalance,
                    InitialCash = dto.InitialBalance,
                },
                HistoricalPositions = new List<HistoricalPosition>(),
                HasPublicPortfolio = true,
            };

            var usernameTaken = await _database.Users.AsQueryable()
                .AnyAsync(u => EF.Functions.Like(u.Username, $"{user.Username}"));
            
            var userUidAlreadyInUse = await _database.Users.AsQueryable()
                .AnyAsync(u => u.Uid == user.Uid);

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
            
            
            if (await _database.Users.AsQueryable().AnyAsync(u => EF.Functions.Like(u.Email, $"{user.Email}")))
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Email already in use",
                    Message = $"The email: {user.Email} is already in use.",
                    UserFriendlyMessage = "Seems like this google account is already linked to another user. Please choose another google account.",
                }, HttpStatusCode.Conflict);
            }

            try
            {
                string updatedProfilePicture = null;
                if (dto.ProfilePictureFile is not null)
                {
                    var result = await CreateUserProfilePictureWithFile(dto.ProfilePictureFile, uid);

                    if (result.TryPickT1(out var error, out var uri))
                        return new ErrorResponse(error, HttpStatusCode.InternalServerError);

                    updatedProfilePicture = uri.AbsoluteUri;
                } else if (dto.ProfilePictureSeed is not null)
                {
                    var result = await CreateUserProfilePictureWithSeed(dto.ProfilePictureSeed, uid);

                    if (result.TryPickT1(out var error, out var uri))
                        return new ErrorResponse(error, HttpStatusCode.InternalServerError);
                
                    updatedProfilePicture = uri.AbsoluteUri;
                }
                
                user.ProfilePictureUrl = updatedProfilePicture;

                var newUser = await _database.Users.AddAsync(user);
                
                _logger.Information("Trying to save new User with uid {@Uid}", uid);
                await _database.SaveChangesAsync();
                _logger.Information("Save User with uid {@Uid} was successful", uid);
                
                return _mapper.Map<UserResponseDto>(newUser.Entity);
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
        }
        
        public async Task<OneOf<UserResponseDto, ErrorResponse>> GetUserById(int id, string uid)
        {
            var user = await _database.Users.FindAsync(id);

            if (user is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "User not found",
                    Message = $"The user with id: {id} could not be found.",
                    UserFriendlyMessage = "The user you are looking for could not be found.",
                }, HttpStatusCode.NotFound);
            }
            
            var userResponse = _mapper.Map<UserResponseDto>(user);

            // If request user is not owner of user hide private information.
            if (user.Uid != uid)
                userResponse.Email = null;

            _logger.Information("User with user uid {@Uid} fetched users with user id {@UserId} successfully", uid, id);
            
            return userResponse;
        }
        
        public async Task<OneOf<UserResponseDto, ErrorResponse>> GetUserByUid(string queriedUid, string uid)
        {
            queriedUid ??= uid;
            
            var user = await _database.Users.AsQueryable().FirstOrDefaultAsync(u => u.Uid == queriedUid);

            if (user is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "User not found",
                    Message = $"The user with uid: {queriedUid} could not be found.",
                    UserFriendlyMessage = "The user you are looking for could not be found.",
                }, HttpStatusCode.NotFound);
            }

            _logger.Information("User with user uid {@Uid} fetched by uid {@QueriedUid} successfully", uid, queriedUid);

            var userResponse = _mapper.Map<UserResponseDto>(user);

            if (uid != user.Uid)
                userResponse.Email = null;

            return userResponse;
        }
        
        public async Task<OneOf<List<UserResponseDto>, ErrorResponse>> SearchUserByUsername(string username, string uid)
        {
            var users = await _database.Users.AsQueryable()
                .Where(u =>  EF.Functions.Like(u.Username, $"{username}%"))
                .ToListAsync();

            _logger.Information("User with user uid {@Uid} fetched {@NumberOfFoundUsers} users by username {@Username} successfully", uid, users.Count, username);
            
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
            
            var user = await _database.Users.AsQueryable().FirstOrDefaultAsync(u => u.Uid == uid);
            
            if (user is null)
                return new ErrorResponse(new NotFound
                {
                    Title = "User not found",
                    Message = $"User with uid: {uid} could not be found.",
                    UserFriendlyMessage = "The user you are looking for could not be found.",
                }, HttpStatusCode.NotFound);
            
            if (dto.Username is not null && await _database.Users
                .AsQueryable().AnyAsync(u => EF.Functions.Like(u.Username, $"{dto.Username}")))
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Username taken",
                    Message = $"The username: {dto.Username} is already in use",
                    UserFriendlyMessage = "The username is unfortunately already in use. Please choose another one.",
                }, HttpStatusCode.Conflict);
            }

            if (dto.Email is not null && await _database.Users
                .AsQueryable().AnyAsync(u => EF.Functions.Like(u.Email, $"{dto.Email}")))
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Email already in use",
                    Message = $"The email: {dto.Email} is already in use.",
                    UserFriendlyMessage = "Seems like this google account is already linked to another user. Please choose another google account.",
                }, HttpStatusCode.Conflict);
            }
            
            string updatedProfilePicture = null;
            if (dto.ProfilePictureFile is not null)
            {
                var result = await CreateUserProfilePictureWithFile(dto.ProfilePictureFile, uid);

                if (result.TryPickT1(out var error, out var uri))
                    return new ErrorResponse(error, HttpStatusCode.InternalServerError);

                updatedProfilePicture = uri.AbsoluteUri;
            } else if (dto.ProfilePictureSeed is not null)
            {
                var result = await CreateUserProfilePictureWithSeed(dto.ProfilePictureSeed, uid);

                if (result.TryPickT1(out var error, out var uri))
                    return new ErrorResponse(error, HttpStatusCode.InternalServerError);
                
                updatedProfilePicture = uri.AbsoluteUri;
            }

            user.Bio = dto.Bio ?? user.Bio;
            user.Email = dto.Email ?? user.Email;
            user.Username = dto.Username ?? user.Username;
            user.DisplayName = dto.DisplayName ?? user.DisplayName;
            user.HasPublicPortfolio = dto.PublicPortfolio ?? user.HasPublicPortfolio;

            string prevProfilePicture = null;
            if (updatedProfilePicture is not null && updatedProfilePicture != user.ProfilePictureUrl)
            {
                prevProfilePicture = user.ProfilePictureUrl;
                user.ProfilePictureUrl = updatedProfilePicture;
            }
            
            try
            {
                _database.Users.Update(user);
                
                _logger.Information("Trying to update user with uid {@Uid}", uid);
                await _database.SaveChangesAsync();
                _logger.Information("Updated User with uid {@Uid} successful", uid);

                if (prevProfilePicture is not null)
                    (await DeleteUserProfilePicture(prevProfilePicture)).Switch(_ => {}, err =>
                    {
                        _logger.Warning(Shared.LogMessageTemplate, err);
                    });
                
                return _mapper.Map<UserResponseDto>(user);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UserUpdateFailed
                {
                    Title = "Failed to save updated user",
                    Message = $"Failed to update user with uid: {uid}",
                    Exception = e,
                    AdditionalData = new { dto }
                }, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<OneOf<DeleteUserResponseDto, ErrorResponse>> DeleteUser(string uid)
        {
            var user = await _database.Users.AsQueryable().FirstOrDefaultAsync(u => u.Uid == uid);

            if (user is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "User not found",
                    Message = $"Could not find user with uid: {uid}",
                }, HttpStatusCode.InternalServerError);
            }
            
            try
            {
                _logger.Information("Trying to delete firebase user with uid {@Uid}", uid);
                await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid, CancellationToken.None);
                _logger.Information("Delete user with uid {@Uid} was successful", uid);
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
            
            try
            {
                _database.Users.Remove(user);
                
                _logger.Information("Trying delete User with uid {@Uid}", uid);
                await _database.SaveChangesAsync();
                _logger.Information("Delete User with uid {@Uid} was successful", uid);
                
                await DeleteUserProfilePicture(user.ProfilePictureUrl);
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

        private async Task<OneOf<Uri, UnexpectedError>> CreateUserProfilePictureWithSeed(string seed, string uid)
        {
            var result = await _cloudinaryService.UploadProfilePicture($"https://avatars.dicebear.com/api/micah/{seed}.svg", uid);

            return result.Match<OneOf<Uri, UnexpectedError>>(uploadResult => uploadResult.SecureUrl, error => error);
        }

        private async Task<OneOf<Uri, UnexpectedError>> CreateUserProfilePictureWithFile(IFormFile file, string uid)
        {
            var result = await _cloudinaryService.UploadProfilePicture(file, uid);

            return result.Match<OneOf<Uri, UnexpectedError>>(uploadResult => uploadResult.SecureUrl, error => error);
        }

        private async Task<OneOf<Success, UnexpectedError>> DeleteUserProfilePicture(string profilePictureUrl)
        {
            return await _cloudinaryService.DeleteImage(profilePictureUrl);
        }
    }
}