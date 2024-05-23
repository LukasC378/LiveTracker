using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(SessionId), IsUnique = true)]
public class SessionResult
{
    [Key]
    public int Id { get; set; }
    public int SessionId { get; set; }
    public required string ResultJson { get; set; }
}