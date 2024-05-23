using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using DB.Database;
using DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace BL.Services;

public class SubscribeService : BaseService, ISubscribeService
{
    private readonly IUserService _userService;
    public SubscribeService(RaceTrackerDbContext dbContext, IUserService userService) : base(dbContext)
    {
        _userService = userService;
    }

    /// <summary>
    /// Returns subscribers of organizer
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<IList<UserVM>> GetSubscribers(Filter filter)
    {
        var organizerId = _userService.GetCurrentUserId();

        if (string.IsNullOrEmpty(filter.SearchTerm))
        {
            return await DbContext.Subscriber.Where(x => x.OrganizerId == organizerId)
                .Join(DbContext.User,
                    s => s.UserId,
                    u => u.Id,
                    (s, u) => new UserVM
                    {
                        Id = u.Id,
                        Name = u.Username
                    })
                .OrderBy(x => x.Name).Skip(filter.Offset).Take(filter.Limit)
                .ToListAsync();
        }
        return await DbContext.Subscriber.Where(x => x.OrganizerId == organizerId)
            .Join(DbContext.User,
                s => s.UserId,
                u => u.Id,
                (s, u) => new UserVM
                {
                    Id = u.Id,
                    Name = u.Username
                })
            .Where(x => DbContext.Unaccent(x.Name).ToLower().Contains(filter.SearchTerm.ToLower()))
            .OrderBy(x => x.Name).Skip(filter.Offset).Take(filter.Limit)
            .ToListAsync();
    }

    public async Task<IList<UserVM>> GetSubscribersForOrganizer(int organizerId)
    {
        return await DbContext.Subscriber.Where(x => x.OrganizerId == organizerId)
            .Join(DbContext.User,
                s => s.UserId,
                u => u.Id,
                (s, u) => new UserVM
                {
                    Id = u.Id,
                    Name = u.Username,
                    Email = u.Email
                })
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Returns subscriptions of user
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<IList<UserVM>> GetSubscriptions(Filter filter)
    {
        var userId = _userService.GetCurrentUserId();

        if (string.IsNullOrEmpty(filter.SearchTerm))
        {
            return await DbContext.Subscriber.Where(x => x.UserId == userId)
                .Join(DbContext.User,
                    s => s.UserId,
                    u => u.Id,
                    (s, u) => new UserVM
                    {
                        Id = u.Id,
                        Name = u.Username
                    })
                .OrderBy(x => x.Name).Skip(filter.Offset).Take(filter.Limit)
                .ToListAsync();
        }
        return await DbContext.Subscriber.Where(x => x.UserId == userId)
            .Join(DbContext.User,
                s => s.UserId,
                u => u.Id,
                (s, u) => new UserVM
                {
                    Id = u.Id,
                    Name = u.Username
                })
            .Where(x => DbContext.Unaccent(x.Name).ToLower().Contains(filter.SearchTerm.ToLower()))
            .OrderBy(x => x.Name).Skip(filter.Offset).Take(filter.Limit)
            .ToListAsync();
    }

    /// <summary>
    /// Add subscribe of organizer
    /// </summary>
    /// <param name="organizerId"></param>
    /// <returns></returns>
    public async Task Subscribe(int organizerId)
    {
        var userId = _userService.GetCurrentUserId();
        var subscriber = new Subscriber
        {
            OrganizerId = organizerId,
            UserId = userId
        };

        await DbContext.AddAsync(subscriber);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Remove subscribe of organizer
    /// </summary>
    /// <param name="organizerId"></param>
    /// <returns></returns>
    public async Task Unsubscribe(int organizerId)
    {
        var userId = _userService.GetCurrentUserId();
        DbContext.RemoveRange(DbContext.Subscriber.Where(x => x.UserId == userId && x.OrganizerId == organizerId));
        await DbContext.SaveChangesAsync();
    }
}