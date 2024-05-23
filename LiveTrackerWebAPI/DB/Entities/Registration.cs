using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(Link), IsUnique = true)]
public class Registration
{
    [Key]
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Link { get; set; }
    public DateTime CreationTime { get; set; }
}