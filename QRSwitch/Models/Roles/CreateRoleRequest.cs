using System.ComponentModel.DataAnnotations;

namespace QRSwitch.Models.Roles
{
    public class CreateRoleRequest
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
