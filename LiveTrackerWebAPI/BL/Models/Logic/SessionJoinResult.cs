namespace BL.Models.Logic;

public class SessionJoinResult
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime ScheduledFrom { get; set; }
    public DateTime ScheduledTo { get; set; }
    public required string Organizer { get; set; }
    public int OrganizerId { get; set; }
    public bool Loaded { get; set; }
    public bool Ended { get; set; }
}