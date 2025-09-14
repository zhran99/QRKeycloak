namespace QRSwitch.Models.Authorization
{
    public class CreateResourceRequest
    {
        public string Name { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? Type { get; set; }
        public List<string>? Scopes { get; set; }
    }
}
