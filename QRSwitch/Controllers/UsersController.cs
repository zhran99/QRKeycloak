using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRSwitch.Models;
using QRSwitch.Models.Users;
using QRSwitch.Models.Responses;
using QRSwitch.Services;
using System.Text.Json;

namespace QRSwitch.Controllers
{
    [ApiController]
    [Route("api")]
    public class UsersController : ControllerBase
    {
        private readonly KeycloakUserService _userService;
        private readonly IConfiguration _config;

        public UsersController(KeycloakUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }
       
        [HttpPost("CreateUser")]
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

     
        [HttpPut("UpdateUser/{userId}")]
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

        [HttpDelete("DeleteUser/{userId}")]
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

       
        [HttpGet("GetAllUsers")]
        //[Permission("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 0, [FromQuery] int size = 20)
        {
            var realm = _config["Keycloak:Realm"] ;
            int first = page * size;
            
            var result = await _userService.GetAllUsersAsync(realm, first, size);

            var response = new GetAllUsersResponse
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                StatusCode = (int)result.StatusCode,
                Pagination = new PaginationDto
                {
                    Page = page,
                    Size = size,
                    First = first
                }
            };

            if (result.Success && !string.IsNullOrEmpty(result.RawResponse))
            {
                try
                {
                    // Debug: Log the raw response
                    Console.WriteLine($"Raw Response: {result.RawResponse}");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var users = JsonSerializer.Deserialize<List<UserDto>>(result.RawResponse, options);
                    response.Data = users ?? new List<UserDto>();
                }
                catch (JsonException ex)
                {
                    response.Success = false;
                    response.ErrorMessage = $"Failed to parse user data: {ex.Message}. Raw response: {result.RawResponse}";
                    response.StatusCode = 500;
                }
            }

            return response.Success ? Ok(response) : BadRequest(response);
        }
        
        [HttpGet("GetUserByUsername/{username}")]
        //[Permission("GetUserByUsername")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var realm = _config["Keycloak:Realm"] ;
            
            var result = await _userService.GetUserByUsernameAsync(realm, username);

            var response = new GetUserByUsernameResponse
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                StatusCode = (int)result.StatusCode
            };

            if (result.Success && !string.IsNullOrEmpty(result.RawResponse))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var users = JsonSerializer.Deserialize<List<UserDto>>(result.RawResponse, options);
                    response.Data = users?.FirstOrDefault();
                }
                catch (JsonException ex)
                {
                    response.Success = false;
                    response.ErrorMessage = $"Failed to parse user data: {ex.Message}";
                    response.StatusCode = 500;
                }
            }

            return response.Success ? Ok(response) : BadRequest(response);
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