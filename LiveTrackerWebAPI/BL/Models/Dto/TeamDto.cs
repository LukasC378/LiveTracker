using BL.Models.Interface;
using DB.Entities;

namespace BL.Models.Dto;

public class TeamDto : IDivisible<Team>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }

    public Team MapToEntity() =>
        new Team
        {
            Name = Name,
            Color = Color
        };
    
    public void UpdateEntity(Team team)
    {
        team.Name = Name;
        team.Color = Color;
    }

    public bool IsEqualToEntity(Team teamDb) =>
        teamDb.Id == Id &&
        teamDb.Name == Name &&
        teamDb.Color == Color;
}