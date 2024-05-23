using System.Data;
using BL.Exceptions;
using BL.Models.Dto;
using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using DB.Database;
using DB.Entities;
using DB.Enums;
using LiveTrackerCommonModels.Dtos;
using LiveTrackerModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static BL.Models.ViewModels.SessionsGroupVM;

namespace BL.Services;

public class SessionService : BaseService, ISessionService
{
    #region Declaration

    private readonly IUserService _userService;
    private readonly ISessionApiService _sessionApiService;
    private readonly IEmailService _emailService;
    private readonly IArchiveService _archiveService;
    private readonly INotificationService _notificationService;

    public SessionService(RaceTrackerDbContext dbContext, IUserService userService, ISessionApiService sessionApiService, IEmailService emailService, IArchiveService archiveService, INotificationService notificationService) : base(dbContext)
    {
        _userService = userService;
        _sessionApiService = sessionApiService;
        _emailService = emailService;
        _archiveService = archiveService;
        _notificationService = notificationService;
    }

    #endregion

    #region Public Methods

    public async Task<SessionToEditDto> GetSessionToEdit(int sessionId)
    {
        var userId = _userService.GetCurrentUserId();
        var query = from s in DbContext.Session
            join c in DbContext.Collection on s.CollectionId equals c.Id
            join l in DbContext.Layout on s.LayoutId equals l.Id into lj
            from layout in lj.DefaultIfEmpty()
            where s.Id == sessionId && s.UserId == userId
            select new SessionToEditDto
            {
                Id = s.Id,
                Name = s.Name,
                CollectionId = s.CollectionId,
                LayoutId = s.LayoutId,
                GeoJson = layout != null ? layout.GeoJson : s.GeoJson,
                ScheduledFrom = s.ScheduledFrom,
                ScheduledTo = s.ScheduledTo,
                Laps = s.Laps,
                CollectionName = c.Name,
                LayoutName = layout != null ? layout.Name : string.Empty
            };
        var sessionToEdit = await query.FirstOrDefaultAsync();
        if (sessionToEdit is null)
            throw new ItemNotFoundException("Session not found");

        return sessionToEdit;
    }

    public async Task<SessionVM> GetSession(int sessionId)
    {
        var session = await DbContext.Session.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session is null)
            throw new ItemNotFoundException("Session not found");

        var collection = await DbContext.Collection.FirstAsync(x => x.Id == session.CollectionId);
        var drivers = await GetSessionDriversVMQueryForCollection(collection).ToListAsync();
        var geoJson = await GetGeoJsonForSession(session);
        var organizer = await DbContext.User.Where(x => x.Id == session.UserId).Select(x => x.Username).FirstOrDefaultAsync();
        if (organizer is null)
            throw new ItemNotFoundException($"Organizer for session {session.Id} not found");

        return new SessionVM
        {
            Name = session.Name,
            ScheduledFrom = session.ScheduledFrom,
            ScheduledTo = session.ScheduledTo,
            Drivers = drivers,
            Organizer = organizer,
            GeoJson = geoJson,
            UseTeams = collection.UseTeams,
            Laps = session.Laps
        };
    }

    public async Task<IEnumerable<SessionToManageVM>> GetSessionsToManage()
    {
        var userId = _userService.GetCurrentUserId();
        return await DbContext.Session
            .Where(x => x.UserId == userId && !x.Loaded && !x.Ended &&
                        x.ScheduledFrom >= DateTime.UtcNow.AddHours(1).AddMinutes(10))
            .Select(x => new SessionToManageVM
            {
                Id = x.Id,
                Name = x.Name,
                ScheduledFrom = x.ScheduledFrom,
                ScheduledTo = x.ScheduledTo,
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionsGroupVM>> GetLiveSessions() =>
        await GetFilteredSessions(query =>
            query.Where(session => session.Loaded && !session.Ended), int.MaxValue, 0);

    public async Task<IEnumerable<SessionsGroupVM>> GetFilteredSessions(SessionFilter filter)
    {
        var today = DateTime.UtcNow.Date;
        Func<IQueryable<SessionJoinResult>, IQueryable<SessionJoinResult>> queryFunc = x =>
        {
            var query = x;

            if (filter.Date is not null)
            {
                query = query.Where(s =>
                    s.ScheduledFrom.Date == filter.Date.Value.Date.ToUniversalTime().AddDays(1).Date);
            }

            if (filter.OrganizerId is not null && filter.OrganizerId is not 0)
            {
                query = query.Where(s => s.OrganizerId == filter.OrganizerId);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(s => DbContext.Unaccent(s.Name).ToLower().Contains(filter.SearchTerm.ToLower()));
            }

            switch (filter.SessionState)
            {
                case SessionStateFilterEnum.Upcoming:
                    query = query.Where(s => s.ScheduledFrom.Date >= DateTime.UtcNow.Date && !s.Loaded && !s.Ended);
                    break;
                case SessionStateFilterEnum.Archived:
                    query = query.Where(s => s.Ended);
                    break;
                case SessionStateFilterEnum.Live:
                    query = query.Where(s => s.Loaded && !s.Ended);
                    break;
                case SessionStateFilterEnum.All:
                    break;
                default:
                    throw new NotSupportedException($"Unknown session state {filter.SessionState}");
            }

            return query;
        };

        return await GetFilteredSessions(queryFunc, filter.Limit, filter.Offset, filter.OrderAsc);
    }

    public async Task<SessionStateEnum> GetSessionState(int sessionId)
    {
        var sessionProps = await DbContext.Session.Where(x => x.Id == sessionId).Select(x => new
        {
            x.Loaded,
            x.Ended
        }).FirstOrDefaultAsync();
        if (sessionProps is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not fount");
        }
        return GetSessionState(sessionProps.Loaded, sessionProps.Ended);
    }

    public async Task<SessionResultVM> GetSessionResult(int sessionId)
    {
        var sessionResultRaw = await DbContext.SessionResult.Where(x => x.SessionId == sessionId)
            .Join(DbContext.Session,
                sr => sr.SessionId,
                s => s.Id,
                (sr, s) => new
                {
                    s.Id,
                    s.Name,
                    sr.ResultJson
                })
            .FirstOrDefaultAsync();

        if (sessionResultRaw is null)
        {
            throw new ItemNotFoundException($"Result for {sessionId} not found");
        }

        var driversResult = JsonConvert.DeserializeObject<IEnumerable<Guid>>(sessionResultRaw.ResultJson);
        if (driversResult is null)
        {
            throw new DataException($"Result for {sessionId} cannot be deserialized");
        }

        var collection = await DbContext.Session.Where(x => x.Id == sessionId)
            .Join(DbContext.Collection,
                s => s.CollectionId,
                c => c.Id,
                (s, c) => c)
            .FirstOrDefaultAsync();
        if (collection is null)
        {
            throw new ItemNotFoundException($"Collection for session {sessionId} not found");
        }

        var driversDict = await GetSessionDriversVMQueryForCollection(collection).ToDictionaryAsync(x => x.CarId, x => x);
        var drivers = driversResult.Select(x => driversDict[x]);

        var result = new SessionResultVM
        {
            Id = sessionResultRaw.Id,
            Name = sessionResultRaw.Name,
            Drivers = drivers
        };

        return result;
    }

    public async Task<IEnumerable<SessionToLoad>> GetSessionsToLoad()
    {
        const int timeOffsetMinutes = 30;

        var sessions = await DbContext.Session
            .Where(x => !x.Loaded && !x.Ended && x.ScheduledFrom <= DateTime.UtcNow.AddMinutes(timeOffsetMinutes))
            .ToListAsync();

        var res = new List<SessionToLoad>();
        foreach (var session in sessions)
        {
            res.Add(new SessionToLoad
            {
                EmailInfo = new SessionEmailVM
                {
                    Id = session.Id,
                    Name = session.Name,
                    ScheduledFrom = session.ScheduledFrom,
                    ScheduledTo = session.ScheduledTo
                },
                RaceData = new RaceDataDto
                {
                    RaceId = session.Id,
                    LapCount = session.Laps,
                    GeoJsonData = await GetGeoJsonForSession(session),
                    Drivers = await GetLiveTrackerDrivers(session.CollectionId)
                },
                Emails = await GetSubscribersEmails(session.UserId)
            });
        }

        return res;
    }

    public async Task<IEnumerable<SessionToUnload>> GetSessionsToUnload()
    {
        const int timeOffsetMinutes = 5;

        //todo <=
        var sessions = await DbContext.Session
            .Where(x => x.Loaded && !x.Ended && x.ScheduledTo <= DateTime.UtcNow.AddMinutes(timeOffsetMinutes))
            .ToListAsync();

        var res = new List<SessionToUnload>();
        foreach (var session in sessions)
        {
            res.Add(new SessionToUnload
            {
                EmailInfo = new SessionEmailVM
                {
                    Id = session.Id,
                    Name = session.Name,
                    ScheduledFrom = session.ScheduledFrom,
                    ScheduledTo = session.ScheduledTo
                },
                Emails = await GetSubscribersEmails(session.UserId)
            });
        }

        return res;
    }


    public async Task CreateSession(SessionDto sessionDto)
    {
        await using var transaction = await DbContext.BeginTransactionAsync();

        try
        {
            var user = await _userService.GetCurrentUser();
            var collectionUserId = await DbContext.Collection.Where(x => x.Id == sessionDto.CollectionId).Select(x => x.UserId).FirstOrDefaultAsync();

            if (collectionUserId is 0)
            {
                throw new ItemNotFoundException($"Collection {sessionDto.CollectionId} not found");
            }
            if (collectionUserId != user.Id)
            {
                throw new UnauthorizedAccessException($"Collection {sessionDto.CollectionId} does not belong to user {user.Id}");
            }
            
            var session = await CreateSession(sessionDto, user.Id);

            await _archiveService.CrateArchiveTable(session.Id);

            var sessionToken = await _sessionApiService.GetSessionToken(new TokenInput
            {
                RaceId = session.Id,
                UserId = user.Id,
                UserName = user.Username,
                ScheduledFrom = session.ScheduledFrom,
                ScheduledTo = session.ScheduledTo
            });

            await _emailService.SendSessionToken(user.Email, sessionToken, session);

            session.Token = sessionToken;

            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateSession(SessionDto sessionDto)
    {
        var user = await _userService.GetCurrentUser();
        var session = await GetSessionWithAuthorization(sessionDto.Id, user.Id);

        if (session.ScheduledFrom < DateTime.UtcNow.AddHours(1))
        {
            throw new InvalidOperationException($"Session {sessionDto.Id} cannot be modified");
        }

        var scheduledFrom = sessionDto.ScheduledFrom.ToUniversalTime();
        var scheduledTo = sessionDto.ScheduledTo.ToUniversalTime();

        if (scheduledFrom != session.ScheduledFrom || scheduledTo != session.ScheduledTo)
        {
            var newToken = await _sessionApiService.GetSessionToken(new TokenInput
            {
                RaceId = sessionDto.Id,
                ScheduledFrom = scheduledFrom,
                ScheduledTo = scheduledTo,
                UserId = user.Id,
                UserName = user.Username
            });

            session.Token = newToken;
        }

        string? geoJson = null;
        int? layoutId = null;
        if (sessionDto.LayoutId is > 0)
            layoutId = sessionDto.LayoutId;
        else
            geoJson = sessionDto.GeoJson;

        session.Name = sessionDto.Name;
        session.ScheduledFrom = scheduledFrom;
        session.ScheduledTo = scheduledTo;
        session.CollectionId = sessionDto.CollectionId;
        session.GeoJson = geoJson;
        session.LayoutId = layoutId;
        session.Laps = sessionDto.Laps;

        await CreateSessionNotification(session.Id, NotificationTypeEnum.Updated);

        await DbContext.SaveChangesAsync();
    }

    public async Task UpdateLoadedSessions(IEnumerable<int> sessionIds)
    {
        var sessionsToUpdate = await DbContext.Session
            .Where(s => sessionIds.Contains(s.Id))
            .ToListAsync();

        foreach (var session in sessionsToUpdate)
        {
            session.Loaded = true;
        }

        await DbContext.SaveChangesAsync();
    }

    public async Task UpdateEndedSessions(List<(int RaceId, IEnumerable<Guid> Result)> results)
    {
        results.ForEach(result =>
        {
            var sessionResult = new SessionResult
            {
                SessionId = result.RaceId,
                ResultJson = JsonConvert.SerializeObject(result.Result)
            };
            DbContext.AddAsync(sessionResult);
        });

        var sessionIds = results.Select(x => x.RaceId);
        await DbContext.Session.Where(x => sessionIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.Ended, true));

        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Cancel session by adding cancellation date
    /// Session will be deleted later by worker
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task CancelSession(int sessionId)
    {
        var userId = _userService.GetCurrentUserId();
        var session = await GetSessionWithAuthorization(sessionId, userId);

        if (session.ScheduledFrom.ToUniversalTime() < DateTime.UtcNow.AddHours(1))
        {
            throw new InvalidOperationException($"Session {sessionId} cannot be cancelled");
        }

        await CreateSessionNotification(session.Id, NotificationTypeEnum.Cancelled);
        DbContext.Remove(session);

        await DbContext.SaveChangesAsync();
    }

    #endregion

    #region Private Methods

    private async Task<IEnumerable<SessionsGroupVM>> GetFilteredSessions(Func<IQueryable<SessionJoinResult>, IQueryable<SessionJoinResult>> filter, int limit, int offset, bool orderAsc = true)
    {
        var sessionQuery = filter(DbContext.Session
            .Join(DbContext.User,
                s => s.UserId,
                u => u.Id,
                (s, u) => new SessionJoinResult
                {
                    Id = s.Id,
                    Name = s.Name,
                    Organizer = u.Username,
                    OrganizerId = u.Id,
                    ScheduledFrom = s.ScheduledFrom,
                    ScheduledTo = s.ScheduledTo,
                    Loaded = s.Loaded,
                    Ended = s.Ended
                }
            ));

        sessionQuery = orderAsc ?
            sessionQuery.OrderBy(x => x.ScheduledFrom) :
            sessionQuery.OrderByDescending(x => x.ScheduledFrom);

        sessionQuery = sessionQuery.Skip(offset).Take(limit);

        var sessions = await sessionQuery.ToListAsync();

        var sessionGroups = sessions
            .GroupBy(x => x.ScheduledFrom.Date)
            .ToDictionary(group => group.Key, group => group);

        var res = sessionGroups.Select(group => new SessionsGroupVM
        {
            Date = group.Key,
            Sessions = group.Value.Select(x =>
            {
                var state = GetSessionState(x.Loaded, x.Ended);

                return new SessionBasicVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Organizer = x.Organizer,
                    ScheduledFrom = x.ScheduledFrom,
                    ScheduledTo = x.ScheduledTo,
                    State = state
                };
            }).ToList()
        });

        var resOrdered = orderAsc ? res.OrderBy(x => x.Date) : res.OrderByDescending(x => x.Date);
        return resOrdered.ToList();
    }

    private async Task<Session> CreateSession(SessionDto sessionDto, int userId)
    {
        string? geoJson = null;
        int? layoutId = null;
        if (sessionDto.LayoutId is > 0)
            layoutId = sessionDto.LayoutId;
        else
            geoJson = sessionDto.GeoJson;

        var session = new Session
        {
            Name = sessionDto.Name,
            UserId = userId,
            ScheduledFrom = sessionDto.ScheduledFrom.ToUniversalTime(),
            ScheduledTo = sessionDto.ScheduledTo.ToUniversalTime(),
            CollectionId = sessionDto.CollectionId,
            GeoJson = geoJson,
            LayoutId = layoutId,
            Laps = sessionDto.Laps,
            CreationDate = DateTime.UtcNow
        };

        await DbContext.AddAsync(session);
        await DbContext.SaveChangesAsync();

        await CreateSessionNotification(session.Id, NotificationTypeEnum.Created);
        return session;
    }

    private async Task CreateSessionNotification(int sessionId, NotificationTypeEnum type)
    {
        await _notificationService.CreateNotification(sessionId, type);
    }

    private static SessionStateEnum GetSessionState(bool loaded, bool ended)
    {
        var state = SessionStateEnum.Preview;
        if (loaded && !ended)
            state = SessionStateEnum.Live;
        else if (ended)
            state = SessionStateEnum.Archived;

        return state;
    }

    private async Task<Session> GetSessionWithAuthorization(int sessionId, int userId)
    {
        var session = await DbContext.Session.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session == null)
        {
            throw new ItemNotFoundException($"Session {sessionId} not found");
        }
        if (session.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        return session;
    }

    private async Task<string> GetGeoJsonForSession(Session session)
    {
        var geoJson = session.GeoJson;
        if (session.LayoutId is > 0)
        {
            geoJson = await DbContext.Layout.Where(x => x.Id == session.LayoutId).Select(x => x.GeoJson).FirstOrDefaultAsync();
            if (geoJson is null)
            {
                throw new ItemNotFoundException($"Layout for session {session.Id} not found");
            }
        }

        return geoJson!;
    }

    private IQueryable<SessionDriverVM> GetSessionDriversVMQueryForCollection(Collection collection)
    {
        IQueryable<SessionDriverVM> driversQuery;
        if (collection.UseTeams)
        {
            driversQuery = DbContext.DriverCollection
                .Where(dc => dc.CollectionId == collection.Id)
                .Join(DbContext.Driver,
                    dc => dc.DriverId,
                    d => d.Id,
                    (dc, d) => d)
                .Join(DbContext.Team,
                    d => d.TeamId,
                    t => t.Id,
                    (d, t) => new SessionDriverVM
                    {
                        Id = d.Id,
                        Name = $"{d.FirstName} {d.LastName}".Trim(),
                        Number = d.Number,
                        Color = t.Color,
                        TeamName = t.Name,
                        CarId = d.GpsDevice
                    });
        }
        else
        {
            driversQuery = DbContext.DriverCollection
                .Where(dc => dc.CollectionId == collection.Id)
                .Join(DbContext.Driver,
                    dc => dc.DriverId,
                    d => d.Id,
                    (dc, d) => new SessionDriverVM
                    {
                        Id = d.Id,
                        Name = $"{d.FirstName} {d.LastName}".Trim(),
                        Number = d.Number,
                        Color = d.Color!,
                        CarId = d.GpsDevice
                    });
        }

        return driversQuery;
    }

    private async Task<IList<LiveTrackerCommonModels.Dtos.DriverDto>> GetLiveTrackerDrivers(int collectionId) =>
        await DbContext.DriverCollection
            .Where(dc => dc.CollectionId == collectionId)
            .Join(DbContext.Driver,
                dc => dc.DriverId,
                d => d.Id,
                (dc, d) => new LiveTrackerCommonModels.Dtos.DriverDto
                {
                    Id = d.GpsDevice
                })
            .ToListAsync();

    private async Task<IList<string>> GetSubscribersEmails(int organizerId) =>
        await DbContext.Subscriber.Where(x => x.OrganizerId == organizerId)
            .Join(DbContext.User,
                s => s.UserId,
                u => u.Id,
                (s, u) => u.Email)
            .ToListAsync();


    

    #endregion
}