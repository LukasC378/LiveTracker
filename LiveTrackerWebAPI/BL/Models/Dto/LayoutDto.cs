namespace BL.Models.Dto;

public class LayoutDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string GeoJson { get; set; }
}