using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
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
            // Todo: Implement Create User handler.
            throw new NotImplementedException();
        }
        
        public async Task<OneOf<User, ErrorResponse>> GetUserById(int id)
        {
            // Todo: Implement Get User handler.
            throw new NotImplementedException();
        }
        
        public async Task<OneOf<IEnumerable<User>, ErrorResponse>> GetUserByUsername(string username)
        {
            // Todo: Implement Get User handler.
            throw new NotImplementedException();
        }
        
        public async Task<OneOf<User, ErrorResponse>> UpdateUser(UpdateUserDto dto)
        {
            // Todo: Implement Get User handler.
            throw new NotImplementedException();
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