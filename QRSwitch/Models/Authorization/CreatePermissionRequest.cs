namespace QRSwitch.Models.Authorization
{
    public class CreatePermissionRequest
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "scope"; // default scope-based
        public List<string> Scopes { get; set; } = new();
        public List<string> Policies { get; set; } = new();
    }
}
