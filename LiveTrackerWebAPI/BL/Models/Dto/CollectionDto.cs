namespace BL.Models.Dto;

public class CollectionDto
{
    public int Id { get; set; }
    public required IList<DriverDto> Drivers { get; set; }
    public IList<TeamDto> Teams { get; set; } = new List<TeamDto>();
    public bool UseTeams { get; set; }
    public string Name { get; set; } = string.Empty;
}