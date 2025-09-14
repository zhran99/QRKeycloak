using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRSwitch.Models;
using QRSwitch.Models.Users;
using QRSwitch.Services;

namespace QRSwitch.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly KeycloakUserService _userService;
        private readonly IConfiguration _config;

        public UsersController(KeycloakUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }
       
        [HttpPost("create")]
        //[Permission("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = await _userService.CreateUserAsync(
                request.Realm, request.Username, request.Email, request.Password, request.FirstName, request.LastName);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    userId = result.UserId,
                    status = result.StatusCode,
                    message = "User created successfully"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

     
        [HttpPut("update/{userId}")]
        //[Permission("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            var realm = _config["Keycloak:Realm"] ;
            
            var result = await _userService.UpdateUserAsync(
                realm, userId, request.Email, request.FirstName, request.LastName, request.Enabled);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    status = result.StatusCode,
                    message = "User updated successfully"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

        [HttpDelete("delete/{userId}")]
        //[Permission("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var realm = _config["Keycloak:Realm"] ;
            
            var result = await _userService.DeleteUserAsync(realm, userId);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    status = result.StatusCode,
                    message = $"User {userId} deleted successfully"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

       
        [HttpGet("all")]
        //[Permission("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 0, [FromQuery] int size = 20)
        {
            var realm = _config["Keycloak:Realm"] ;
            int first = page * size;
            
            var result = await _userService.GetAllUsersAsync(realm, first, size);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    data = result.RawResponse,
                    pagination = new { page, size, first }
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }
        
        [HttpGet("by-username/{username}")]
        //[Permission("GetUserByUsername")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var realm = _config["Keycloak:Realm"] ;
            
            var result = await _userService.GetUserByUsernameAsync(realm, username);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    data = result.RawResponse
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }
       
        [HttpPost("reset-password/{userId}")]
        //[Permission("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetPasswordRequest request)
        {
            var realm = _config["Keycloak:Realm"] ;
            
            var result = await _userService.ResetUserPasswordAsync(realm, userId, request.NewPassword);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    status = result.StatusCode,
                    message = $"Password reset successfully for user {userId}"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

    }
}