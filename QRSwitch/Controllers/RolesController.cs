using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRSwitch.Models;
using QRSwitch.Models.Roles;
using QRSwitch.Models.Responses;
using QRSwitch.Services;
using System.Text.Json;
using static QRSwitch.Services.KeycloakRoleService;

namespace QRSwitch.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly KeycloakRoleService _roleService;
        private readonly IConfiguration _config;

        public RolesController(KeycloakRoleService roleService, IConfiguration config)
        {
            _roleService = roleService;
            _config = config;
        }

        [HttpPost("create")]
        //[Permission("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var realm = _config["Keycloak:Realm"];

            var result = await _roleService.CreateRoleAsync(realm, request.RoleName, request.Description);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    status = result.StatusCode,
                    message = $"Role '{request.RoleName}' created successfully"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

        [HttpPut("update/{roleName}")]
        //[Permission("UpdateRole")]
        public async Task<IActionResult> UpdateRole(string roleName, [FromBody] UpdateRoleRequest request)
        {
            var realm = _config["Keycloak:Realm"];

            var result = await _roleService.UpdateRoleAsync(realm, roleName, request.Description);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    status = result.StatusCode,
                    message = $"Role '{roleName}' updated successfully"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

        [HttpDelete("delete/{roleName}")]
        //[Permission("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var realm = _config["Keycloak:Realm"];

            var result = await _roleService.DeleteRoleAsync(realm, roleName);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    status = result.StatusCode,
                    message = $"Role '{roleName}' deleted successfully"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

        [HttpGet("all")]
        //[Permission("CreateRole")]
        public async Task<IActionResult> GetAllRoles()
        {
            var realm = _config["Keycloak:Realm"];

            var result = await _roleService.GetAllRolesAsync(realm);

            var response = new GetAllRolesResponse
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
                    var roles = JsonSerializer.Deserialize<List<RoleDto>>(result.RawResponse, options);
                    response.Data = roles ?? new List<RoleDto>();
                }
                catch (JsonException ex)
                {
                    response.Success = false;
                    response.ErrorMessage = $"Failed to parse role data: {ex.Message}";
                    response.StatusCode = 500;
                }
            }

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("user/{username}")]
        //[Permission("GetAllRoles")]
        public async Task<IActionResult> GetRolesForUser(string username)
        {
            var realm = _config["Keycloak:Realm"];

            var result = await _roleService.GetRolesForUserByUsernameAsync(realm, username);

            var response = new GetRolesForUserResponse
            {
                Success = result.Success,
                Username = username,
                ErrorMessage = result.ErrorMessage,
                StatusCode = (int)result.StatusCode
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
                    var roles = JsonSerializer.Deserialize<List<RoleDto>>(result.RawResponse, options);
                    response.Roles = roles ?? new List<RoleDto>();
                }
                catch (JsonException ex)
                {
                    response.Success = false;
                    response.ErrorMessage = $"Failed to parse role data: {ex.Message}. Raw response: {result.RawResponse}";
                    response.StatusCode = 500;
                }
            }

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("assign")]
        //[Permission("AssignRoleToUser")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
        {
            var realm = _config["Keycloak:Realm"];

            var result = await _roleService.AssignRoleToUserAsync(realm, request.UserId, request.RoleName);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    status = result.StatusCode,
                    message = $"Role '{request.RoleName}' assigned to user {request.UserId} successfully"
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

        [HttpPost("create-multiple")]
        //[Permission("CreateMultipleRoles")]
        public async Task<IActionResult> CreateMultipleRoles([FromBody] CreateRolesRequest request)
        {
            var realm = _config["Keycloak:Realm"];
            if (request.Roles == null || !request.Roles.Any())
                return BadRequest("No roles provided");

            var results = await _roleService.CreateRolesAsync(request.Roles, realm);
            return Ok(results);
        }



    }
}
