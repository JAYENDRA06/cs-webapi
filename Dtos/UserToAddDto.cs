namespace DotnetAPI.Dtos;

// Data Transfer Object = DTO
// Since in the AddUser controller we dont need to provide UserId but if we use User model it's expected to give UserId, we can make a DTO model for this prupose
public partial class UserToAddDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Gender { get; set; } = "";
    public bool Active { get; set; }
}
