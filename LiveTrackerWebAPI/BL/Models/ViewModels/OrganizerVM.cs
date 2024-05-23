namespace BL.Models.ViewModels;

public class OrganizerVM
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool Subscribed { get; set; }
}