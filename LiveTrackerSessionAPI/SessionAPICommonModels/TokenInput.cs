namespace SessionAPICommonModels;

public class TokenInput
{
    public required int RaceId { get; set; }
    public required int UserId { get; set; }
    public required string UserName { get; set; }
    public required DateTime ScheduledFrom { get; set; }
    public required DateTime ScheduledTo { get; set; }
}