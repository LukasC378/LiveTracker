using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("subscribe")]
public class SubscribeController : Controller
{
    #region Declaration

    private readonly ISubscribeService _subscribeService;

    public SubscribeController(ISubscribeService subscribeService)
    {
        _subscribeService = subscribeService;
    }

    #endregion

    #region GET Methods

    [Authorize(Roles = UserRole.Organizer)]
    [HttpGet("subscribers")]
    public async Task<IList<UserVM>> GetSubscribers(Filter filter)
    {
        return await _subscribeService.GetSubscribers(filter);
    }

    [Authorize(Roles = UserRole.NormalUser)]
    [HttpGet("subscriptions")]
    public async Task<IList<UserVM>> GetSubscriptions(Filter filter)
    {
        return await _subscribeService.GetSubscriptions(filter);
    }

    #endregion

    #region POST Methods

    [Authorize(Roles = UserRole.NormalUser)]
    [HttpPost("subscribe")]
    public async Task Subscribe([FromForm] int organizerId)
    {
        await _subscribeService.Subscribe(organizerId);
    }

    #endregion

    #region DELETE Methods

    [Authorize(Roles = UserRole.NormalUser)]
    [HttpDelete("unsubscribe")]
    public async Task Unsubscribe(int organizerId)
    {
        await _subscribeService.Unsubscribe(organizerId);
    }

    #endregion

}