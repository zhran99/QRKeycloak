namespace QRSwitch.Models.Users
{
    public record CreateUserRequest(string Realm, string Username, string Email, string Password, string FirstName, string LastName);
}
