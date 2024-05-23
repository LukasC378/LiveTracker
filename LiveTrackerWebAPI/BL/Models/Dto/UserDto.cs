using DB.Enums;

namespace BL.Models.Dto;

public class UserDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public UserRoleEnum Role { get; set; }
}

public class UserToRegisterDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Link { get; set; }
    public required UserRoleEnum Role { get; set; }
}