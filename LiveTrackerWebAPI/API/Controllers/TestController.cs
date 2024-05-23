﻿using BL;
using BL.Models.Dto;
using BL.Models.Logic;
using BL.Services.Interfaces;
using DB.Database;
using DB.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("test")]
public class TestController : Controller
{
    private readonly ICollectionService _collectionService;
    private readonly ISessionService _sessionService;
    private readonly IRegistrationService _registrationService;
    private readonly INotificationService _notificationService;
    private RaceTrackerDbContext DbContext { get; }

    public TestController(ICollectionService collectionService, ISessionService sessionService,
        IRegistrationService registrationService, RaceTrackerDbContext dbContext, INotificationService notificationService)
    {
        _collectionService = collectionService;
        _sessionService = sessionService;
        _registrationService = registrationService;
        DbContext = dbContext;
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task Test([FromBody] IList<Guid> drivers)
    {
        var driversDto = drivers.Select((t, i) => new DriverDto
            {
                Color = "#ff0000",
                GpsDevice = t,
                Name = "Test",
                Number = i,
                Surname = i.ToString()
            })
            .ToList();

        var collectionDto = new CollectionDto
        {
            Drivers = driversDto,
            UseTeams = false
        };
        await _collectionService.CreateCollection(collectionDto);
    }

    [HttpPost("collections")]
    public async Task TestCollections()
    {
        var guids = new List<Guid>();
        for (var i = 0; i < 10; i++)
        {
            guids.Add(Guid.NewGuid());
        }

        var driversDto = guids.Select((t, i) => new DriverDto
            {
                Color = "#ff0000",
                GpsDevice = t,
                Name = "Test",
                Number = i,
                Surname = i.ToString()
            })
            .ToList();

        var collectionDto = new CollectionDto
        {
            Drivers = driversDto,
            UseTeams = false
        };
        await _collectionService.CreateCollection(collectionDto);
    }

    [HttpPost("sessions")]
    public async Task TestSessions()
    {
        // SPA
        const string geoJson =
            "{\n\"type\": \"FeatureCollection\",\n\"name\": \"be-1925\",\n\"bbox\": [ 5.959602, 50.427678, 5.977560, 50.446217 ], \n\"features\": [\n{ \"type\": \"Feature\", \"properties\": { \"id\": \"be-1925\", \"Location\": \"Spa Francorchamps\", \"Name\": \"Circuit de Spa-Francorchamps\", \"opened\": 1925, \"firstgp\": 1950, \"length\": 7004, \"altitude\": 413 }, \"bbox\": [ 5.959602, 50.427678, 5.97756, 50.446217 ], \"geometry\": { \"type\": \"LineString\", \"coordinates\": [ [ 5.96502, 50.444251 ], [ 5.963419, 50.446033 ], [ 5.963402, 50.446113 ], [ 5.963473, 50.446184 ], [ 5.963621, 50.446217 ], [ 5.963786, 50.446188 ], [ 5.964313, 50.446019 ], [ 5.965592, 50.445628 ], [ 5.966207, 50.445387 ], [ 5.966847, 50.445085 ], [ 5.967421, 50.444779 ], [ 5.967876, 50.444463 ], [ 5.970321, 50.442606 ], [ 5.970493, 50.442502 ], [ 5.970788, 50.442385 ], [ 5.971315, 50.442168 ], [ 5.971546, 50.442022 ], [ 5.971741, 50.441824 ], [ 5.971866, 50.441644 ], [ 5.971949, 50.441442 ], [ 5.97202, 50.441069 ], [ 5.972061, 50.440937 ], [ 5.972132, 50.440815 ], [ 5.972268, 50.440655 ], [ 5.973476, 50.439424 ], [ 5.974245, 50.438642 ], [ 5.974458, 50.43835 ], [ 5.974594, 50.438133 ], [ 5.974754, 50.437784 ], [ 5.975719, 50.435639 ], [ 5.977199, 50.432382 ], [ 5.977542, 50.431599 ], [ 5.97756, 50.431472 ], [ 5.977524, 50.431331 ], [ 5.977406, 50.431218 ], [ 5.977234, 50.431123 ], [ 5.977015, 50.431048 ], [ 5.976885, 50.430968 ], [ 5.976796, 50.430874 ], [ 5.976725, 50.430732 ], [ 5.976737, 50.430591 ], [ 5.977033, 50.429747 ], [ 5.977027, 50.429601 ], [ 5.97698, 50.429469 ], [ 5.97682, 50.429323 ], [ 5.97663, 50.429224 ], [ 5.973257, 50.427739 ], [ 5.973044, 50.427682 ], [ 5.972831, 50.427678 ], [ 5.972606, 50.42772 ], [ 5.972422, 50.427805 ], [ 5.972292, 50.427927 ], [ 5.972239, 50.42805 ], [ 5.972227, 50.428182 ], [ 5.972292, 50.428305 ], [ 5.97241, 50.428432 ], [ 5.972582, 50.428521 ], [ 5.974056, 50.429101 ], [ 5.974216, 50.429205 ], [ 5.974304, 50.429309 ], [ 5.97434, 50.429431 ], [ 5.974322, 50.429582 ], [ 5.973712, 50.430723 ], [ 5.973523, 50.431189 ], [ 5.973091, 50.432627 ], [ 5.972878, 50.433452 ], [ 5.972831, 50.433593 ], [ 5.972724, 50.433744 ], [ 5.972582, 50.433867 ], [ 5.972369, 50.433999 ], [ 5.972132, 50.434098 ], [ 5.971872, 50.434164 ], [ 5.971599, 50.434192 ], [ 5.970717, 50.43423 ], [ 5.97038, 50.434206 ], [ 5.970072, 50.43415 ], [ 5.969759, 50.434065 ], [ 5.969504, 50.433956 ], [ 5.969208, 50.433782 ], [ 5.969019, 50.433608 ], [ 5.968812, 50.433358 ], [ 5.967977, 50.432028 ], [ 5.967231, 50.430845 ], [ 5.967071, 50.430661 ], [ 5.966882, 50.430534 ], [ 5.966622, 50.43044 ], [ 5.966361, 50.430393 ], [ 5.966119, 50.430388 ], [ 5.965876, 50.430421 ], [ 5.965669, 50.430482 ], [ 5.965325, 50.430624 ], [ 5.9651, 50.430685 ], [ 5.964828, 50.430713 ], [ 5.964556, 50.430694 ], [ 5.964307, 50.430643 ], [ 5.964094, 50.430548 ], [ 5.963958, 50.43044 ], [ 5.963792, 50.430294 ], [ 5.962425, 50.428927 ], [ 5.962289, 50.428847 ], [ 5.962123, 50.42879 ], [ 5.961922, 50.428762 ], [ 5.961697, 50.428771 ], [ 5.961502, 50.428828 ], [ 5.960578, 50.429257 ], [ 5.960034, 50.429511 ], [ 5.959898, 50.429624 ], [ 5.959756, 50.429761 ], [ 5.959673, 50.429893 ], [ 5.959614, 50.430049 ], [ 5.959602, 50.430209 ], [ 5.959643, 50.430402 ], [ 5.959738, 50.430567 ], [ 5.959862, 50.430779 ], [ 5.960046, 50.4311 ], [ 5.960365, 50.431463 ], [ 5.960715, 50.431779 ], [ 5.961247, 50.43216 ], [ 5.962135, 50.432712 ], [ 5.962656, 50.432971 ], [ 5.9631, 50.433136 ], [ 5.965385, 50.433895 ], [ 5.965716, 50.434027 ], [ 5.96603, 50.434183 ], [ 5.96632, 50.434357 ], [ 5.966568, 50.434546 ], [ 5.966799, 50.434744 ], [ 5.967107, 50.435106 ], [ 5.967356, 50.435455 ], [ 5.967924, 50.436261 ], [ 5.968024, 50.436436 ], [ 5.968084, 50.436624 ], [ 5.968095, 50.43686 ], [ 5.968036, 50.437072 ], [ 5.967699, 50.437624 ], [ 5.967332, 50.438232 ], [ 5.966888, 50.438991 ], [ 5.966775, 50.439273 ], [ 5.966669, 50.439608 ], [ 5.966604, 50.439919 ], [ 5.966562, 50.440183 ], [ 5.966432, 50.441404 ], [ 5.966456, 50.441484 ], [ 5.966533, 50.441541 ], [ 5.966651, 50.44156 ], [ 5.966852, 50.441541 ], [ 5.96706, 50.441541 ], [ 5.967202, 50.44156 ], [ 5.967296, 50.441626 ], [ 5.96732, 50.441701 ], [ 5.967261, 50.4418 ], [ 5.966533, 50.442559 ], [ 5.96502, 50.444251 ] ] } }\n]\n}";
        var random = new Random();

        for (var i = 0; i < 5; i++)
        {
            var session = new SessionDto
            {
                Name = "Test" + i,
                CollectionId = 1,
                GeoJson = geoJson,
                Laps = random.Next(1, 10),
                ScheduledFrom = DateTime.UtcNow.AddDays(random.Next(1, 9)),
                ScheduledTo = DateTime.UtcNow.AddDays(random.Next(10, 19)),
            };
            await _sessionService.CreateSession(session);
        }
    }

    [HttpPost("seedOrganizers")]
    public async Task SeedOrganizers()
    {
        for (var i = 0; i < 10; i++)
        {
            try
            {
                var name = "Organizer" + i;
                var email = $"organizer{i}@test.com";
                var registrationFirstResult = await _registrationService.SendRegistrationLinkAsync(email);
                if (registrationFirstResult != RegistrationFirstResultEnum.Ok)
                {
                    continue;
                }

                var link = (await DbContext.Registration.FirstAsync(x => x.Email == email)).Link;
                await _registrationService.RegisterUserAsync(new UserToRegisterDto
                {
                    Link = link,
                    Username = name,
                    Password = name,
                    Role = UserRoleEnum.Organizer
                });
            }
            catch (Exception ex)
            {
                continue;
            }
        }
    }


    [HttpGet("notifications")]
    public async Task<IList<NotificationDetail>> GetNotifications()
    {
        return await _notificationService.LoadNewNotificationsWithDetails();
    }

    [HttpGet("myUser")]
    public async Task MyUser()
    {
        try
        {
            var name = "User";
            var email = "lu.cauner@gmail.com";
            var registrationFirstResult = await _registrationService.SendRegistrationLinkAsync(email);
            if (registrationFirstResult != RegistrationFirstResultEnum.Ok)
            {
                throw new Exception("registration first failed");
            }

            var link = (await DbContext.Registration.FirstAsync(x => x.Email == email)).Link;
            await _registrationService.RegisterUserAsync(new UserToRegisterDto
            {
                Link = link,
                Username = name,
                Password = name,
                Role = UserRoleEnum.NormalUser
            });
        }
        catch (Exception ex)
        {
            var a = ex.Message;
        }
    }
}