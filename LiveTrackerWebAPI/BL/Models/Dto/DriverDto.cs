using BL.Models.Interface;
using DB.Entities;

namespace BL.Models.Dto;

public class DriverDto : IDivisible<Driver>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Surname { get; set; } = string.Empty;
    public int Number { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Color { get; set; }
    public Guid GpsDevice { get; set; }

    public Driver MapToEntity() =>
        new()
        {
            FirstName = Name,
            LastName = Surname,
            Number = Number,
            TeamId = TeamId,
            Color = (TeamId is not null ? null : Color),
            GpsDevice = GpsDevice
        };

    public void UpdateEntity(Driver driver)
    {
        driver.FirstName = Name;
        driver.LastName = Surname;
        driver.Number = Number;
        driver.TeamId = TeamId;
        driver.Color = (TeamId is not null ? null : Color);
        driver.GpsDevice = GpsDevice;
    }

    public bool IsEqualToEntity(Driver driver) =>
        Name == driver.FirstName &&
        Surname == driver.LastName &&
        Number == driver.Number &&
        TeamId == driver.TeamId &&
        (TeamId is not null ? null : Color) == driver.Color &&
        GpsDevice == driver.GpsDevice;
}