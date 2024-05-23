using BL.Models.Dto;
using BL.Services;
using BL.Services.Interfaces;
using DB.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.ServicesTests;

public class CollectionServiceTests : BaseMock
{
    private readonly ICollectionService _collectionService;
    private readonly Mock<IUserService> _userServiceMock = new();

    private readonly int _userId;

    public CollectionServiceTests()
    {
        _collectionService = new CollectionService(DbContext, _userServiceMock.Object);

        var user = AddUser().GetAwaiter().GetResult();
        _userId = user.Id;
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(1);
        _userServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
    }

    [Fact]
    public async Task CreateCollection_Test()
    {
        var driver1Dto = new DriverDto
        {
            Name = "Driver1",
            Surname = "Surname1",
            Color = "red",
            Number = 1,
            GpsDevice = Guid.NewGuid()
        };
        var driver2Dto = new DriverDto
        {
            Name = "Driver2",
            Surname = "Surname2",
            Color = "blue",
            Number = 2,
            GpsDevice = Guid.NewGuid()
        };

        var collectionDto = new CollectionDto
        {
            Name = "TestCollection",
            Drivers = new List<DriverDto> { driver1Dto, driver2Dto },
            UseTeams = false
        };

        await _collectionService.CreateCollection(collectionDto);

        var collection = DbContext.Collection.FirstOrDefault();
        Assert.NotNull(collection);
        Assert.Equal(_userId, collection.UserId);
        Assert.Equal(false, collection.UseTeams);

        var drivers = DbContext.Driver.ToList();
        var driversCollections = DbContext.DriverCollection.ToList();

        Assert.Equal(2, drivers.Count);
        Assert.Equal(2, driversCollections.Count);

        var driver1 = drivers.FirstOrDefault(x => x.Number == driver1Dto.Number);
        var driver2 = drivers.FirstOrDefault(x => x.Number == driver2Dto.Number);

        Assert.NotNull(driver1);
        Assert.NotNull(driver2);

        Assert.Equal(driver1Dto.Name, driver1.FirstName);
        Assert.Equal(driver1Dto.Surname, driver1.LastName);
        Assert.Equal(driver1Dto.Color, driver1.Color);
        Assert.Equal(driver1Dto.GpsDevice, driver1.GpsDevice);

        Assert.Equal(driver2Dto.Name, driver2.FirstName);
        Assert.Equal(driver2Dto.Surname, driver2.LastName);
        Assert.Equal(driver2Dto.Color, driver2.Color);
        Assert.Equal(driver2Dto.GpsDevice, driver2.GpsDevice);
    }

    [Fact]
    public async Task GetCollection_Test()
    {
        var team1Dto = new TeamDto
        {
            Name = "Team1",
            Color = "red"
        };
        var team2Dto = new TeamDto
        {
            Name = "Team2",
            Color = "blue"
        };
        var driver1Dto = new DriverDto
        {
            Name = "Driver1",
            Surname = "Surname1",
            TeamName = "Team1",
            Number = 1,
            GpsDevice = Guid.NewGuid()
        };
        var driver2Dto = new DriverDto
        {
            Name = "Driver2",
            Surname = "Surname2",
            TeamName = "Team2",
            Number = 2,
            GpsDevice = Guid.NewGuid()
        };
        var collectionDto = new CollectionDto
        {
            Name = "TestCollection",
            Drivers = new List<DriverDto> { driver1Dto, driver2Dto },
            Teams = new List<TeamDto>{ team1Dto, team2Dto },
            UseTeams = true
        };

        await _collectionService.CreateCollection(collectionDto);

        var collection = await DbContext.Collection.FirstOrDefaultAsync();
        Assert.NotNull(collection);

        var collectionVM = await _collectionService.GetCollection(collection.Id);
        Assert.NotNull(collectionVM);
        Assert.Equal(collectionDto.Name, collectionVM.Name);
        Assert.Equal(collectionDto.Drivers.Count, collectionVM.Drivers.Count);
        Assert.Equal(collectionDto.Teams.Count, collectionVM.Teams.Count);

        var driver1 = collectionVM.Drivers.FirstOrDefault(x => x.Number == driver1Dto.Number);
        var driver2 = collectionVM.Drivers.FirstOrDefault(x => x.Number == driver2Dto.Number);
        Assert.NotNull(driver1);
        Assert.NotNull(driver2);

        Assert.Equal(driver1Dto.Name, driver1.Name);
        Assert.Equal(driver1Dto.Surname, driver1.Surname);
        Assert.Equal(team1Dto.Color, driver1.Color);
        Assert.Equal(driver1Dto.GpsDevice, driver1.GpsDevice);

        Assert.Equal(driver2Dto.Name, driver2.Name);
        Assert.Equal(driver2Dto.Surname, driver2.Surname);
        Assert.Equal(team2Dto.Color, driver2.Color);
        Assert.Equal(driver2Dto.GpsDevice, driver2.GpsDevice);
    }

    [Fact]
    public async Task CollectionUpdate_UnusedCollection()
    {
        var team1Dto = new TeamDto
        {
            Name = "Team1",
            Color = "red"
        };
        var team2Dto = new TeamDto
        {
            Name = "Team2",
            Color = "blue"
        };
        var driver1Dto = new DriverDto
        {
            Name = "Driver1",
            Surname = "Surname1",
            TeamName = "Team1",
            Number = 1,
            GpsDevice = Guid.NewGuid()
        };
        var driver2Dto = new DriverDto
        {
            Name = "Driver2",
            Surname = "Surname2",
            TeamName = "Team2",
            Number = 2,
            GpsDevice = Guid.NewGuid()
        };
        var collectionDto = new CollectionDto
        {
            Name = "TestCollection",
            Drivers = new List<DriverDto> { driver1Dto, driver2Dto },
            Teams = new List<TeamDto> { team1Dto, team2Dto },
            UseTeams = true
        };

        await _collectionService.CreateCollection(collectionDto);
        var collection = await DbContext.Collection.FirstOrDefaultAsync();
        Assert.NotNull(collection);

        var collectionToEdit = await _collectionService.GetCollection(collection.Id);
        Assert.NotNull(collectionToEdit);

        var driver1ToEdit = collectionToEdit.Drivers.First(x => x.Number == driver1Dto.Number);
        var driver2ToEdit = collectionToEdit.Drivers.First(x => x.Number == driver2Dto.Number);
        driver2ToEdit.TeamName = "Team1";
        collectionToEdit.Drivers.Remove(driver1ToEdit);

        await _collectionService.UpdateCollection(collectionToEdit);

        var drivers = await DbContext.Driver.ToListAsync();
        Assert.Equal(1, drivers.Count);
        var teams = await DbContext.Team.ToListAsync();
        Assert.Equal(2, teams.Count);

        var driver = drivers[0];
        var team1 = teams.FirstOrDefault(x => x.Name == "Team1");
        Assert.NotNull(team1);
        Assert.Equal(2, driver.Number);
        Assert.Equal(team1.Id, driver.TeamId);
    }

    [Fact]
    public async Task CollectionUpdate_UsedCollection()
    {
        var team1Dto = new TeamDto
        {
            Name = "Team1",
            Color = "red"
        };
        var team2Dto = new TeamDto
        {
            Name = "Team2",
            Color = "blue"
        };
        var driver1Dto = new DriverDto
        {
            Name = "Driver1",
            Surname = "Surname1",
            TeamName = "Team1",
            Number = 1,
            GpsDevice = Guid.NewGuid()
        };
        var driver2Dto = new DriverDto
        {
            Name = "Driver2",
            Surname = "Surname2",
            TeamName = "Team2",
            Number = 2,
            GpsDevice = Guid.NewGuid()
        };
        var collectionDto = new CollectionDto
        {
            Name = "TestCollection",
            Drivers = new List<DriverDto> { driver1Dto, driver2Dto },
            Teams = new List<TeamDto> { team1Dto, team2Dto },
            UseTeams = true
        };

        await _collectionService.CreateCollection(collectionDto);
        var collection = await DbContext.Collection.FirstOrDefaultAsync();
        Assert.NotNull(collection);

        var session = new Session
        {
            CollectionId = collection.Id,
            GeoJson = "geoJson",
            CreationDate = DateTime.Now,
            Laps = 2,
            Name = "TestSession",
            ScheduledFrom = DateTime.Now,
            ScheduledTo = DateTime.Now.AddDays(1)
        };
        await DbContext.AddAsync(session);
        await DbContext.SaveChangesAsync();

        var collectionToEdit = await _collectionService.GetCollection(collection.Id);
        Assert.NotNull(collectionToEdit);

        var driver1ToEdit = collectionToEdit.Drivers.First(x => x.Number == driver1Dto.Number);
        var driver2ToEdit = collectionToEdit.Drivers.First(x => x.Number == driver2Dto.Number);
        driver2ToEdit.Name = "NewDriver";
        driver2ToEdit.TeamName = "Team1";
        collectionToEdit.Drivers.Remove(driver1ToEdit);
        collectionToEdit.Name = "NewCollection";

        await _collectionService.UpdateCollection(collectionToEdit);

        var collections = DbContext.Collection.ToList();
        Assert.Equal(2, collections.Count);

        var oldCollection = collections.FirstOrDefault(x => x.Name == "TestCollection");
        var newCollection = collections.FirstOrDefault(x => x.Name == "NewCollection");
        Assert.NotNull(oldCollection);
        Assert.NotNull(newCollection);

        var teams = await DbContext.Team.ToListAsync();
        Assert.Equal(2, teams.Count);

        var team1 = teams.FirstOrDefault(x => x.Name == "Team1");
        var team2 = teams.FirstOrDefault(x => x.Name == "Team2");
        Assert.NotNull(team1);
        Assert.NotNull(team2);
        Assert.Equal(team1Dto.Name, team1.Name);
        Assert.Equal(team1Dto.Color, team1.Color);
        Assert.Equal(team2Dto.Name, team2.Name);
        Assert.Equal(team2Dto.Color, team2.Color);

        var teamCollections = await DbContext.TeamCollection.ToListAsync();
        Assert.Equal(4, teamCollections.Count);
        Assert.NotNull(teamCollections.FirstOrDefault(x => x.TeamId == team1.Id && x.CollectionId == oldCollection.Id));
        Assert.NotNull(teamCollections.FirstOrDefault(x => x.TeamId == team2.Id && x.CollectionId == oldCollection.Id));
        Assert.NotNull(teamCollections.FirstOrDefault(x => x.TeamId == team1.Id && x.CollectionId == newCollection.Id));
        Assert.NotNull(teamCollections.FirstOrDefault(x => x.TeamId == team2.Id && x.CollectionId == newCollection.Id));

        var drivers = await DbContext.Driver.ToListAsync();
        Assert.Equal(3, drivers.Count);

        var oldDriver1 = drivers.FirstOrDefault(x => x.FirstName == "Driver1");
        var oldDriver2 = drivers.FirstOrDefault(x => x.FirstName == "Driver2");
        var newDriver = drivers.FirstOrDefault(x => x.FirstName == "NewDriver");
        Assert.NotNull(oldDriver1);
        Assert.NotNull(oldDriver2);
        Assert.NotNull(newDriver);

        Assert.Equal(driver1Dto.Name, oldDriver1.FirstName);
        Assert.Equal(driver1Dto.Surname, oldDriver1.LastName);
        Assert.Equal(driver1Dto.GpsDevice, oldDriver1.GpsDevice);
        Assert.Equal(driver1Dto.TeamName, team1.Name);

        Assert.Equal(driver2Dto.Name, oldDriver2.FirstName);
        Assert.Equal(driver2Dto.Surname, oldDriver2.LastName);
        Assert.Equal(driver2Dto.GpsDevice, oldDriver2.GpsDevice);
        Assert.Equal(driver2Dto.TeamName, team2.Name);

        Assert.Equal(driver2ToEdit.Name, newDriver.FirstName);
        Assert.Equal(driver2ToEdit.Surname, newDriver.LastName);
        Assert.Equal(driver2ToEdit.GpsDevice, newDriver.GpsDevice);
        Assert.Equal(driver2ToEdit.TeamName, team1.Name);

        var driversCollections = await DbContext.DriverCollection.ToListAsync();
        Assert.Equal(3, driversCollections.Count);
        Assert.NotNull(driversCollections.FirstOrDefault(x => x.DriverId == oldDriver1.Id && x.CollectionId == oldCollection.Id));
        Assert.NotNull(driversCollections.FirstOrDefault(x => x.DriverId == oldDriver2.Id && x.CollectionId == oldCollection.Id));
        Assert.NotNull(driversCollections.FirstOrDefault(x => x.DriverId == newDriver.Id && x.CollectionId == newCollection.Id));
    }

    [Fact]
    public async Task CollectionDelete_UnusedCollection()
    {
        var team1Dto = new TeamDto
        {
            Name = "Team1",
            Color = "red"
        };
        var team2Dto = new TeamDto
        {
            Name = "Team2",
            Color = "blue"
        };
        var driver1Dto = new DriverDto
        {
            Name = "Driver1",
            Surname = "Surname1",
            TeamName = "Team1",
            Number = 1,
            GpsDevice = Guid.NewGuid()
        };
        var driver2Dto = new DriverDto
        {
            Name = "Driver2",
            Surname = "Surname2",
            TeamName = "Team2",
            Number = 2,
            GpsDevice = Guid.NewGuid()
        };
        var collectionDto = new CollectionDto
        {
            Name = "TestCollection",
            Drivers = new List<DriverDto> { driver1Dto, driver2Dto },
            Teams = new List<TeamDto> { team1Dto, team2Dto },
            UseTeams = true
        };

        await _collectionService.CreateCollection(collectionDto);
        var collection = await DbContext.Collection.FirstOrDefaultAsync();
        Assert.NotNull(collection);

        await _collectionService.DeleteCollection(collection.Id);

        var collections = await DbContext.Collection.ToListAsync();
        var drivers = await DbContext.Driver.ToListAsync();
        var teams = await DbContext.Team.ToListAsync();
        var driversCollections = await DbContext.DriverCollection.ToListAsync();
        var teamsCollections = await DbContext.TeamCollection.ToListAsync();

        Assert.Empty(collections);
        Assert.Empty(drivers);
        Assert.Empty(teams);
        Assert.Empty(driversCollections);
        Assert.Empty(teamsCollections);
    }

    [Fact]
    public async Task CollectionDelete_UsedCollection()
    {
        var team1Dto = new TeamDto
        {
            Name = "Team1",
            Color = "red"
        };
        var team2Dto = new TeamDto
        {
            Name = "Team2",
            Color = "blue"
        };
        var driver1Dto = new DriverDto
        {
            Name = "Driver1",
            Surname = "Surname1",
            TeamName = "Team1",
            Number = 1,
            GpsDevice = Guid.NewGuid()
        };
        var driver2Dto = new DriverDto
        {
            Name = "Driver2",
            Surname = "Surname2",
            TeamName = "Team2",
            Number = 2,
            GpsDevice = Guid.NewGuid()
        };
        var collectionDto = new CollectionDto
        {
            Name = "TestCollection",
            Drivers = new List<DriverDto> { driver1Dto, driver2Dto },
            Teams = new List<TeamDto> { team1Dto, team2Dto },
            UseTeams = true
        };

        await _collectionService.CreateCollection(collectionDto);
        var collection = await DbContext.Collection.FirstOrDefaultAsync();
        Assert.NotNull(collection);

        var session = new Session
        {
            CollectionId = collection.Id,
            GeoJson = "geoJson",
            CreationDate = DateTime.Now,
            Laps = 2,
            Name = "TestSession",
            ScheduledFrom = DateTime.Now,
            ScheduledTo = DateTime.Now.AddDays(1)
        };
        await DbContext.AddAsync(session);
        await DbContext.SaveChangesAsync();

        await _collectionService.DeleteCollection(collection.Id);

        var collections = await DbContext.Collection.ToListAsync();
        var drivers = await DbContext.Driver.ToListAsync();
        var teams = await DbContext.Team.ToListAsync();
        var driversCollections = await DbContext.DriverCollection.ToListAsync();
        var teamsCollections = await DbContext.TeamCollection.ToListAsync();

        Assert.Equal(1, collections.Count);
        Assert.Equal(2, drivers.Count);
        Assert.Equal(2, teams.Count);
        Assert.Equal(2, driversCollections.Count);
        Assert.Equal(2, teamsCollections.Count);
    }
}