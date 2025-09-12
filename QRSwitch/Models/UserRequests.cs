using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QRSwitch.Models
{
    #region Authentication Requests
    public record LoginRequest(string Username, string Password);
    #endregion

    #region User Management Requests
    public record CreateUserRequest(string Realm, string Username, string Email, string Password, string FirstName, string LastName);
    
    public class UpdateUserRequest
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? Enabled { get; set; }
    }

    public record ResetPasswordRequest(string NewPassword);
    #endregion

    #region Role Management Requests
    public class CreateRoleRequest
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string? Description { get; set; }
    }

    public class AssignRoleRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
    #endregion

    #region Models
    public class KeycloakUser
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool Enabled { get; set; }
    }
    #endregion

    #region roles/permessions
    public class CreateScopeDto
    {
        public string Name { get; set; }
    }

    public class CreateResourceDto
    {
        public string Name { get; set; }
        public List<string> Scopes { get; set; }
    }

    public class CreateRolePolicyDto
    {
        public string PolicyName { get; set; }
        public string RoleName { get; set; }
    }

    public class CreatePermissionDto
    {
        public string PermissionName { get; set; }
        public string ScopeName { get; set; }
        public List<string> PolicyNames { get; set; }
    }
    #endregion

    #region Authrazation
    public class CreateScopeRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;
       
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
      
        [JsonPropertyName("iconUri")]
        public string? IconUri { get; set; }
    }

    public class CreateResourceRequest
    {
        public string Name { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? Type { get; set; }
        public List<string>? Scopes { get; set; }
    }

    public class CreatePolicyRequest
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "role"; // default role policy
        public List<string> Roles { get; set; } = new();
    }

    public class CreatePermissionRequest
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "scope"; // default scope-based
        public List<string> Scopes { get; set; } = new();
        public List<string> Policies { get; set; } = new();
    } 
    #endregion


}