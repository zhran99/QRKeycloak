using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRSwitch.Models;
using QRSwitch.Services;
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

        #region Role CRUD Operations

        [HttpPost("create")]
        [Permission("CreateRole")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var realm = _config["Keycloak:Realm"] ;
            
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
        //[Authorize(Policy = "UpdateRole")]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> UpdateRole(string roleName, [FromBody] UpdateRoleRequest request)
        {
            var realm = _config["Keycloak:Realm"] ;
            
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
        //[Authorize(Policy = "DeleteRole")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var realm = _config["Keycloak:Realm"] ;
            
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
        //[Authorize(Policy = "ViewRoles")]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> GetAllRoles()
        {
            var realm = _config["Keycloak:Realm"] ;
            
            var result = await _roleService.GetAllRolesAsync(realm);

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

        #endregion

        #region User-Role Assignment

        
        [HttpGet("user/{username}")]
        [Permission("ViewUserRoles")]
        public async Task<IActionResult> GetRolesForUser(string username)
        {
            var realm = _config["Keycloak:Realm"] ;
            
            var result = await _roleService.GetRolesForUserByUsernameAsync(realm, username);

            return result.Success
                ? Ok(new
                {
                    success = true,
                    username = username,
                    roles = result.RawResponse
                })
                : BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    status = result.StatusCode
                });
        }

        [HttpPost("assign")]
        //[Authorize(Policy = "AssignRole")]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
        {
            var realm = _config["Keycloak:Realm"] ;
            
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
        public async Task<IActionResult> CreateMultipleRoles([FromBody] CreateRolesRequest request)
        {
            var realm = _config["Keycloak:Realm"];
            if (request.Roles == null || !request.Roles.Any())
                return BadRequest("No roles provided");

            var results = await _roleService.CreateRolesAsync(request.Roles,realm);
            return Ok(results);
        }
        #endregion
        #region MAnage Permessions and Roles

        [HttpPost("create-resource")]
        public async Task<IActionResult> CreateResource(string realm, string clientId)
        {
            var result = await _roleService.CreateResourceAsync(
                realm,
                clientId,
                "User",
                new List<string> { "create", "update", "delete" }
            );
            return Ok(result);
        }

        [HttpPost("create-policy")]
        public async Task<IActionResult> CreatePolicy(string realm, string clientId, string policyName, string roleId)
        {
            var result = await _roleService.CreateRolePolicyAsync(realm, clientId, policyName, roleId);
            return Ok(result);
        }

        [HttpPost("create-permission")]
        public async Task<IActionResult> CreatePermission(string realm, string clientId, string permissionName, string resourceName, [FromBody] PermissionRequest request)
        {
            var result = await _roleService.CreatePermissionAsync(realm, clientId, permissionName, resourceName, request.Scopes, request.Policies);
            return Ok(result);
        }

        public class PermissionRequest
        {
            public List<string> Scopes { get; set; }
            public List<string> Policies { get; set; }
        }

        #endregion

        
    }
}
