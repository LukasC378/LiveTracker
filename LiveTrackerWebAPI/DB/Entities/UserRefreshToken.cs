using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(UserId)), Index(nameof(RefreshToken))]
public class UserRefreshToken
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ValidTo { get; set; }
}