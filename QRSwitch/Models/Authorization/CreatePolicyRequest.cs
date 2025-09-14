namespace QRSwitch.Models.Authorization
{
    public class CreatePolicyRequest
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "role";
        public List<string> Roles { get; set; } = new();
    }
}
