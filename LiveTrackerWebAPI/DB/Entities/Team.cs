using System.ComponentModel.DataAnnotations;

namespace DB.Entities;

public class Team
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
}