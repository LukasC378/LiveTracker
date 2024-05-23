using System.ComponentModel.DataAnnotations;
using DB.Enums;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(Username), IsUnique = true), Index(nameof(Email)), Index(nameof(Role))]
public class User
{
    [Key]
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public required string PasswordSalt { get; set; }
    public required UserRoleEnum Role { get; set; }
}