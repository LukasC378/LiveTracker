using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(TeamId)), Index(nameof(CollectionId))]
public class TeamCollection
{
    [Key]
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int CollectionId { get; set; }
}