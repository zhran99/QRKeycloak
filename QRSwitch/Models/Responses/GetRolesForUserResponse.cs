namespace QRSwitch.Models.Responses
{
    public class GetRolesForUserResponse : BaseResponse
    {
        public string Username { get; set; } = null!;
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }
}
