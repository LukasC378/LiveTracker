using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(TeamId))]
public class Driver
{
    [Key]
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public int Number { get; set; }
    public int? TeamId { get; set; }
    public string? Color { get; set; }
    public Guid GpsDevice { get; set; }
}