using System.ComponentModel.DataAnnotations;

namespace QRSwitch.Models.Roles
{
    public class AssignRoleRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}
