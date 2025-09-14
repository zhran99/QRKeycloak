using System.ComponentModel.DataAnnotations;

namespace QRSwitch.Models.Users
{
    public class UpdateUserRequest
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? Enabled { get; set; }
    }
}
