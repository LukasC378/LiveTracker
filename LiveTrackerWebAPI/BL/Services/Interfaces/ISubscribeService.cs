using BL.Models.Logic;
using BL.Models.ViewModels;

namespace BL.Services.Interfaces;

public interface ISubscribeService
{
    /// <summary>
    /// Returns subscribers of organizer
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<IList<UserVM>> GetSubscribers(Filter filter);

    /// <summary>
    /// Returns subscribers of organizer
    /// </summary>
    /// <param name="organizerId"></param>
    /// <returns></returns>
    Task<IList<UserVM>> GetSubscribersForOrganizer(int organizerId);

    /// <summary>
    /// Returns subscriptions of user
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<IList<UserVM>> GetSubscriptions(Filter filter);

    /// <summary>
    /// Add subscribe of organizer
    /// </summary>
    /// <param name="organizerId"></param>
    /// <returns></returns>
    Task Subscribe(int organizerId);

    /// <summary>
    /// Remove subscribe of organizer
    /// </summary>
    /// <param name="organizerId"></param>
    /// <returns></returns>
    Task Unsubscribe(int organizerId);
}