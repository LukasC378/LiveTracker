using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(UserId)), Index(nameof(Loaded)), Index(nameof(Ended)), Index(nameof(CollectionId)), 
 Index(nameof(ScheduledFrom)), Index(nameof(ScheduledTo)), Index(nameof(LayoutId))]
public class Session
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime ScheduledFrom { get; set; }
    public DateTime ScheduledTo { get; set; }
    public int UserId { get; set; }
    public string? GeoJson { get; set; }
    public int? LayoutId { get; set; }
    public int CollectionId { get; set; }
    public int Laps { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public bool Loaded { get; set; }
    public bool Ended { get; set; }
}