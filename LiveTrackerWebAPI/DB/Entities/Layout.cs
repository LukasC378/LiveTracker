using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(UserId))]
public class Layout
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string GeoJson { get; set; }
    public int UserId { get; set; }
    public bool Active { get; set; }
}