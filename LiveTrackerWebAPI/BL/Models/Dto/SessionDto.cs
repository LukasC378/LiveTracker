namespace BL.Models.Dto;

public class SessionDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int CollectionId { get; set; }
    public int? LayoutId { get; set; }
    public string? GeoJson { get; set; }
    public DateTime ScheduledFrom { get; set; }
    public DateTime ScheduledTo { get; set; }
    public int Laps { get; set; }
}

public class SessionToEditDto : SessionDto
{
    public required string CollectionName { get; set; }
    public string LayoutName { get; set; } = string.Empty;
}