using System.Data;
using BL.Exceptions;
using BL.Models.Dto;
using BL.Models.Interface;
using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using DB.Database;
using DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace BL.Services;

public class CollectionService : BaseService, ICollectionService
{
    #region Declaration

    private readonly IUserService _userService;

    public CollectionService(RaceTrackerDbContext dbContext, IUserService userService) : base(dbContext) 
    {
        _userService = userService;
    }

    #endregion

    #region Public Methods

    public async Task<IList<CollectionBasicVM>> GetCollections()
    {
        var userId = _userService.GetCurrentUserId();
        return await DbContext.Collection.Where(x => x.UserId == userId && x.Active).Select(x => new CollectionBasicVM
        {
            Id = x.Id,
            Name = x.Name
        }).ToListAsync();
    }

    public async Task<CollectionDto> GetCollection(int collectionId)
    {
        var userId = _userService.GetCurrentUserId();
        var collectionDb = await GetCollectionWithAuthorization(collectionId, userId);

        IList<DriverDto> drivers;
        IList<TeamDto> teams = new List<TeamDto>();

        if (!collectionDb.UseTeams)
        {
            drivers = await GetDriverDtoListForCollectionId(collectionId);
        }
        else
        {
            drivers = await GetDriverDtoListWithTeamsForCollectionId(collectionId);
            teams = await GetTeamDtoListForCollectionId(collectionId);
        }

        return new CollectionDto
        {
            Id = collectionDb.Id,
            Name = collectionDb.Name,
            UseTeams = collectionDb.UseTeams,
            Drivers = drivers,
            Teams = teams
        };
    }

    public async Task CreateCollection(CollectionDto collectionDto)
    {
        await using var transaction = await DbContext.BeginTransactionAsync();

        try
        {
            var userId = _userService.GetCurrentUserId();
            var collection = await InsertCollection(collectionDto.Name, collectionDto.UseTeams, userId);

            if (!collectionDto.UseTeams)
            {
                await InsertDriversWithRelations(collectionDto.Drivers, collection.Id);
            }
            else
            {
                if (!collectionDto.Teams.Any())
                {
                    throw new DataException("Teams cannot be empty");
                }
                var teams = await InsertTeamsWithRelations(collectionDto.Teams, collection.Id);
                var teamNameIdDict = teams.ToDictionary(x => x.Name, x => x.Id);
                await InsertDriversWithTeams(collectionDto.Drivers, collection.Id, teamNameIdDict);
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateCollection(CollectionDto collectionDto)
    {
        await using var transaction = await DbContext.BeginTransactionAsync();

        try
        {
            var userId = _userService.GetCurrentUserId();
            var collectionDb  = await GetCollectionWithAuthorization(collectionDto.Id, userId);

            #region Collection

            var isOldCollectionUsed = await DbContext.Session.CountAsync(x => x.CollectionId == collectionDto.Id) > 0;
            var newCollectionId = collectionDto.Id;

            if (isOldCollectionUsed)
            {
                newCollectionId = (await InsertCollection(collectionDto.Name, collectionDto.UseTeams, userId)).Id;
                collectionDb.Active = false;
            }
            else
            {
                collectionDb.Name = collectionDto.Name;
                collectionDb.UseTeams = collectionDto.UseTeams;
            }
            //changes will be saved later

            #endregion

            #region Teams

            var teamsUserActionDivision = await DivideByUserAction<TeamDto, Team>(collectionDto.Teams, collectionDto.Id);
            var teamsDbActionDivision = await DivideByDbAction<TeamDto, Team>(teamsUserActionDivision, collectionDto.Id, isOldCollectionUsed);

            if (teamsDbActionDivision.ToInsert.Any())
                await DbContext.AddRangeAsync(teamsDbActionDivision.ToInsert);

            //execute insert, update
            await DbContext.SaveChangesAsync();

            var currentTeamsIds = new List<int>();

            if (collectionDto.UseTeams)
            {
                var teamsNameIdDict = new Dictionary<string, int>();

                foreach (var team in teamsDbActionDivision.ToInsert)
                {
                    teamsNameIdDict[team.Name] = team.Id;
                    currentTeamsIds.Add(team.Id);
                }
                foreach (var team in teamsDbActionDivision.ToUpdate)
                {
                    teamsNameIdDict[team.Name] = team.Id;
                    currentTeamsIds.Add(team.Id);
                }
                foreach (var team in teamsUserActionDivision.NotChanged)
                {
                    teamsNameIdDict[team.Name] = team.Id;
                    currentTeamsIds.Add(team.Id);
                }

                //update drivers teams relations
                foreach (var driverDto in collectionDto.Drivers)
                {
                    if (teamsNameIdDict.TryGetValue(driverDto.TeamName!, out var teamId))
                    {
                        driverDto.TeamId = teamId;
                    }
                }
            }

            //teams collection
            await UpdateTeamCollection(currentTeamsIds, teamsDbActionDivision, newCollectionId, isOldCollectionUsed);

            //delete teams
            if (teamsDbActionDivision.ToDelete.Any())
                DbContext.RemoveRange(teamsDbActionDivision.ToDelete);


            #endregion

            #region Drivers

            var driversUserActionDivision = await DivideByUserAction<DriverDto, Driver>(collectionDto.Drivers, collectionDto.Id);
            var driversDbActionDivision = await DivideByDbAction<DriverDto, Driver>(driversUserActionDivision, collectionDto.Id, isOldCollectionUsed);

            if (driversDbActionDivision.ToInsert.Any())
                await DbContext.AddRangeAsync(driversDbActionDivision.ToInsert);

            //execute insert, update
            await DbContext.SaveChangesAsync();


            var insertedDriversIds = driversDbActionDivision.ToInsert.Select(s => s.Id);
            var updatedDriversIds = driversDbActionDivision.ToUpdate.Select(s => s.Id);
            var notChangedDriversIds = driversUserActionDivision.NotChanged.Select(s => s.Id);
            var driversIds = insertedDriversIds.Concat(updatedDriversIds).Concat(notChangedDriversIds);

            //driver collection
            await UpdateDriverCollection(driversIds, driversDbActionDivision, newCollectionId, isOldCollectionUsed);

            //delete drivers
            if (driversDbActionDivision.ToDelete.Any())
                DbContext.RemoveRange(driversDbActionDivision.ToDelete);

            #endregion

            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RenameCollection(int collectionId, string collectionName)
    {
        var userId = _userService.GetCurrentUserId();
        var collection = await GetCollectionWithAuthorization(collectionId, userId);

        collection.Name = collectionName;

        await DbContext.SaveChangesAsync();
    }

    public async Task DeleteCollection(int collectionId)
    {
        var userId = _userService.GetCurrentUserId();
        var collection = await GetCollectionWithAuthorization(collectionId, userId);

        var isCollectionUsed = await DbContext.Session.CountAsync(x => x.CollectionId == collectionId) > 0;

        if (isCollectionUsed)
        {
            collection.Active = false;
        }
        else
        {
            var unusedTeamsQuery = DbContext.Team
                .Where(team =>
                    DbContext.TeamCollection.Count(tc => tc.TeamId == team.Id) == 1 &&
                    DbContext.TeamCollection.Any(tc => tc.TeamId == team.Id && tc.CollectionId == collectionId));

            var unusedDriversQuery = DbContext.Driver
                .Where(driver =>
                    DbContext.DriverCollection.Count(dc => dc.DriverId == driver.Id) == 1 &&
                    DbContext.DriverCollection.Any(dc => dc.DriverId == driver.Id && dc.CollectionId == collectionId));

            var teamsRelationsQuery = DbContext.TeamCollection.Where(x => x.CollectionId == collectionId);
            var driversRelationsQuery = DbContext.TeamCollection.Where(x => x.CollectionId == collectionId);

            DbContext.RemoveRange(teamsRelationsQuery);
            DbContext.RemoveRange(driversRelationsQuery);

            DbContext.RemoveRange(unusedTeamsQuery);
            DbContext.RemoveRange(unusedDriversQuery);

            DbContext.Remove(collection);
        }

        await DbContext.SaveChangesAsync();
    }

    #endregion

    #region Private Methods

    #region Select

    private async Task<IList<DriverDto>> GetDriverDtoListForCollectionId(int collectionId) =>
        await DbContext.DriverCollection
            .Where(dc => dc.CollectionId == collectionId)
            .Join(DbContext.Driver,
                dc => dc.DriverId,
                d => d.Id,
                (dc, d) => new DriverDto
                {
                    Name = d.FirstName,
                    Surname = d.LastName,
                    Color = d.Color,
                    GpsDevice = d.GpsDevice,
                    Number = d.Number,
                })
            .OrderBy(x => x.Number)
            .ToListAsync();

    private async Task<IList<DriverDto>> GetDriverDtoListWithTeamsForCollectionId(int collectionId) =>
        await DbContext.DriverCollection
            .Where(dc => dc.CollectionId == collectionId)
            .Join(DbContext.Driver,
                dc => dc.DriverId,
                d => d.Id,
                (dc, d) => d)
            .Join(DbContext.Team, driver => driver.TeamId, team => team.Id, (driver, team) => new DriverDto
            {
                Id = driver.Id,
                Name = driver.FirstName,
                Surname = driver.LastName,
                Color = team.Color,
                GpsDevice = driver.GpsDevice,
                Number = driver.Number,
                TeamName = team.Name,
            })
            .OrderBy(x => x.Number)
            .ToListAsync();

    private async Task<IList<TeamDto>> GetTeamDtoListForCollectionId(int collectionId) =>
        await DbContext.TeamCollection
            .Where(tc => tc.CollectionId == collectionId)
            .Join(DbContext.Team,
                tc => tc.TeamId,
                t => t.Id,
                (tc, t) => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Color = t.Color
                })
            .OrderBy(x => x.Name)
            .ToListAsync();

    #endregion

    #region Insert

    private async Task<Collection> InsertCollection(string name, bool useTeams, int userId)
    {
        var collection = new Collection
        {
            Active = true,
            Name = name,
            UserId = userId,
            UseTeams = useTeams
        };
        await DbContext.AddAsync(collection);
        await DbContext.SaveChangesAsync();

        return collection;
    }

    private async Task<IEnumerable<Team>> InsertTeamsWithRelations(IEnumerable<TeamDto> teamsDto, int collectionId)
    {
        var teams = teamsDto.Select(x => new Team
        {
            Name = x.Name,
            Color = x.Color,
        }).ToList();

        await DbContext.AddRangeAsync(teams);
        await DbContext.SaveChangesAsync();

        var teamCollectionList = teams.Select(x => new TeamCollection
        {
            CollectionId = collectionId,
            TeamId = x.Id
        }).ToList();

        await DbContext.AddRangeAsync(teamCollectionList);
        await DbContext.SaveChangesAsync();

        return teams;
    }

    private async Task InsertTeamCollection(IEnumerable<int> teamsIds, int collectionId)
    {
        var teamCollectionList = teamsIds.Select(x => new TeamCollection
        {
            CollectionId = collectionId,
            TeamId = x
        }).ToList();


        await DbContext.AddRangeAsync(teamCollectionList);
        await DbContext.SaveChangesAsync();
    }

    private async Task InsertDriversWithTeams(IEnumerable<DriverDto> driverDto, int collectionId, IDictionary<string, int> teamNameIdDict)
    {
        var drivers = driverDto.Select(x => new Driver
        {
            FirstName = x.Name,
            LastName = x.Surname,
            Number = x.Number,
            TeamId = teamNameIdDict[x.TeamName!],
            Color = null,
            GpsDevice = x.GpsDevice
        }).ToList();

        await DbContext.AddRangeAsync(drivers);
        await DbContext.SaveChangesAsync();

        await InsertDriverCollection(drivers, collectionId);
    }

    private async Task InsertDriversWithRelations(IEnumerable<DriverDto> driverDto, int collectionId)
    {
        var drivers = driverDto.Select(x => new Driver
        {
            FirstName = x.Name,
            LastName = x.Surname,
            Number = x.Number,
            TeamId = null,
            Color = x.Color,
            GpsDevice = x.GpsDevice
        }).ToList();

        await DbContext.AddRangeAsync(drivers);
        await DbContext.SaveChangesAsync();

        await InsertDriverCollection(drivers, collectionId);
    }

    private async Task InsertDriverCollection(IEnumerable<Driver> drivers, int collectionId)
    {
        var driverCollectionList = drivers.Select(x => new DriverCollection
        {
            CollectionId = collectionId,
            DriverId = x.Id
        }).ToList();

        await DbContext.AddRangeAsync(driverCollectionList);
        await DbContext.SaveChangesAsync();
    }

    private async Task InsertDriverCollection(IEnumerable<int> driversIds, int collectionId)
    {
        var driverCollectionList = driversIds.Select(x => new DriverCollection
        {
            CollectionId = collectionId,
            DriverId = x
        }).ToList();

        await DbContext.AddRangeAsync(driverCollectionList);
    }

    #endregion

    #region Update

    private async Task UpdateTeamCollection(IEnumerable<int> currentTeamsIds, DbActionDivision<Team> teamsDbActionDivision, int collectionId, bool isOldCollectionUsed)
    {
        if (isOldCollectionUsed)
        {
            await InsertTeamCollection(currentTeamsIds, collectionId);
            return;
        }
        if (teamsDbActionDivision.ToDelete.Any())
        {
            var teamsIdsToDelete = teamsDbActionDivision.ToDelete.Select(x => x.Id);
            var entitiesToDeleteQuery = DbContext.TeamCollection.Where(x => teamsIdsToDelete.Contains(x.TeamId));
            DbContext.TeamCollection.RemoveRange(entitiesToDeleteQuery);
        }
        if (teamsDbActionDivision.ToInsert.Any())
        {
            await InsertTeamCollection(teamsDbActionDivision.ToInsert.Select(x => x.Id), collectionId);
        }
    }

    private async Task UpdateDriverCollection(IEnumerable<int> currentDriversIds, DbActionDivision<Driver> driversDbActionDivision, int collectionId, bool isOldCollectionUsed)
    {
        if (isOldCollectionUsed)
        {
            await InsertDriverCollection(currentDriversIds, collectionId);
            return;
        }
        if (driversDbActionDivision.ToDelete.Any())
        {
            var driversIdsToDelete = driversDbActionDivision.ToDelete.Select(x => x.Id);
            var entitiesToDeleteQuery = DbContext.DriverCollection.Where(x => driversIdsToDelete.Contains(x.DriverId));
            DbContext.DriverCollection.RemoveRange(entitiesToDeleteQuery);
        }
        if (driversDbActionDivision.ToInsert.Any())
        {
            await InsertDriverCollection(driversDbActionDivision.ToInsert.Select(x => x.Id), collectionId);
        }
    }

    #endregion

    #region Logic

    private async Task<Collection> GetCollectionWithAuthorization(int collectionId, int userId)
    {
        var collection = await DbContext.Collection.FirstOrDefaultAsync(x => x.Id == collectionId);
        if (collection == null)
        {
            throw new ItemNotFoundException($"Collection {collectionId} not found");
        }
        if (userId != collection.UserId)
        {
            throw new UnauthorizedAccessException();
        }
        return collection;
    }


    private async Task<UserActionDivision<T1>> DivideByUserAction<T1, T2>(IList<T1> items, int collectionId) where T1 : IDivisible<T2>
    {
        var ret = new UserActionDivision<T1>();

        IDictionary<int, T2> oldItemsDict;
        if (typeof(T2) == typeof(Team))
        {
            oldItemsDict = (await DbContext.TeamCollection
                .Where(tc => tc.CollectionId == collectionId)
                .Join(DbContext.Team,
                    tc => tc.TeamId,
                    t => t.Id,
                    (tc, t) => t)
                .ToDictionaryAsync(x => x.Id, y => y) as IDictionary<int, T2>)!;
        }
        else if (typeof(T2) == typeof(Driver))
        {
            oldItemsDict = (await DbContext.DriverCollection
                .Where(tc => tc.CollectionId == collectionId)
                .Join(DbContext.Driver,
                    dc => dc.DriverId,
                    d => d.Id,
                    (dc, d) => d)
                .ToDictionaryAsync(x => x.Id, y => y) as IDictionary<int, T2>)!;
        }
        else
        {
            throw new DataException($"Unsupported division type {typeof(T2)}");
        }

        //deleted
        var currentIds = items.Where(x => x.Id != 0).Select(x => x.Id);
        ret.Deleted = oldItemsDict.Select(x => x.Key).Except(currentIds).ToList();

        foreach (var item in items)
        {
            //new 
            if (item.Id == 0)
            {
                ret.Created.Add(item);
                continue;
            }

            //unknown
            if (!oldItemsDict.TryGetValue(item.Id, out var oldDriver))
                continue;

            //not changed
            if (item.IsEqualToEntity(oldDriver))
            {
                ret.NotChanged.Add(item);
                continue;
            }

            //updated
            ret.Updated.Add(item);
        }

        return ret;
    }

    private async Task<DbActionDivision<T2>> DivideByDbAction<T1, T2>(UserActionDivision<T1> items, int collectionId, bool isCollectionUsed) where T1 : IDivisible<T2>
    {
        var ret = new DbActionDivision<T2>
        {
            ToInsert = items.Created.Select(x => x.MapToEntity()).ToList()
        };

        if (!isCollectionUsed)
        {
            var itemsToCheckIds = items.Updated.Select(x => x.Id).Concat(items.Deleted).ToArray();
            if (itemsToCheckIds.Any())
            {
                IDictionary<int, T2> unusedItemsDict;
                if (typeof(T2) == typeof(Team))
                {
                    unusedItemsDict = (await DbContext.Team
                        .Where(team =>
                            itemsToCheckIds.Contains(team.Id) &&
                            DbContext.TeamCollection.Count(tc => tc.TeamId == team.Id) == 1 &&
                            DbContext.TeamCollection.Any(tc => tc.TeamId == team.Id && tc.CollectionId == collectionId))
                        .ToDictionaryAsync(x => x.Id, y => y) as IDictionary<int, T2>)!;
                }
                else if (typeof(T2) == typeof(Driver))
                {
                    unusedItemsDict = (await DbContext.Driver
                        .Where(driver =>
                            itemsToCheckIds.Contains(driver.Id) &&
                            DbContext.DriverCollection.Count(dc => dc.DriverId == driver.Id) == 1 &&
                            DbContext.DriverCollection.Any(dc => dc.DriverId == driver.Id && dc.CollectionId == collectionId))
                        .ToDictionaryAsync(x => x.Id, y => y) as IDictionary<int, T2>)!;
                }
                else
                {
                    throw new DataException($"Unsupported division type {typeof(T2)}");
                }

                foreach (var item in items.Updated)
                {
                    if (unusedItemsDict.TryGetValue(item.Id, out var teamToUpdate))
                    {
                        item.UpdateEntity(teamToUpdate);
                        ret.ToUpdate.Add(teamToUpdate);
                    }
                    else
                    {
                        ret.ToInsert.Add(item.MapToEntity());
                    }
                }

                foreach (var itemId in items.Deleted)
                {
                    if (unusedItemsDict.TryGetValue(itemId, out var itemToDelete))
                    {
                        ret.ToDelete.Add(itemToDelete);
                    }
                }
            }
        }
        else
        {
            foreach (var item in items.Updated)
            {
                ret.ToInsert.Add(item.MapToEntity());
            }
        }

        return ret;
    }

    #endregion

    #endregion
}