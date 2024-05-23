using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(UserId)), Index(nameof(Active))]
public class Collection
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    public int UserId { get; set; }

    public bool UseTeams { get; set; }

    public bool Active { get; set; }
}