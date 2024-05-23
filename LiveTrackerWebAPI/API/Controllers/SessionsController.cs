using BL;
using BL.Models.Dto;
using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BL.Models.ViewModels.SessionsGroupVM;

namespace API.Controllers;

[Route("sessions")]
public class SessionsController : Controller
{
    #region Declaration

    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    #endregion

    #region GET Methods

    [HttpGet("{sessionId:int}")]
    public async Task<SessionVM> GetSession(int sessionId)
    {
        return await _sessionService.GetSession(sessionId);
    }

    [Authorize(Roles = UserRole.Organizer)]
    [HttpGet("edit/{sessionId:int}")]
    public async Task<SessionToEditDto> GetSessionToEdit(int sessionId)
    {
        return await _sessionService.GetSessionToEdit(sessionId);
    }

    [Authorize(Roles = UserRole.Organizer)]
    [HttpGet("manage")]
    public async Task<IEnumerable<SessionToManageVM>> GetSessionsToManage()
    {
        return await _sessionService.GetSessionsToManage();
    }


    [HttpGet("live")]
    public async Task<IEnumerable<SessionsGroupVM>> GetLiveSessions()
    {
        return await _sessionService.GetLiveSessions();
    }

    [HttpGet("state/{sessionId:int}")]
    public async Task<SessionStateEnum> GetSessionState(int sessionId)
    {
        return await _sessionService.GetSessionState(sessionId);
    }

    [HttpGet("result/{sessionId:int}")]
    public async Task<SessionResultVM> GetSessionResult(int sessionId)
    {
        return await _sessionService.GetSessionResult(sessionId);
    }

    #endregion

    #region POST Methods

    [Authorize(Roles = UserRole.Organizer)]
    [HttpPost]
    public async Task CreateSession([FromBody] SessionDto sessionDto)
    {
        await _sessionService.CreateSession(sessionDto);
    }

    [HttpPost("filter")]
    public async Task<IEnumerable<SessionsGroupVM>> GetFilteredSessions([FromBody] SessionFilter filer)
    {
        return await _sessionService.GetFilteredSessions(filer);
    }

    #endregion

    #region PUT Methods

    [Authorize(Roles = UserRole.Organizer)]
    [HttpPut]
    public async Task UpdateSession([FromBody] SessionDto sessionDto)
    {
        await _sessionService.UpdateSession(sessionDto);
    }

    #endregion

    #region DELETE Methods

    [Authorize(Roles = UserRole.Organizer)]
    [HttpDelete("cancel/{sessionId:int}")]
    public async Task CancelSession(int sessionId)
    {
        await _sessionService.CancelSession(sessionId);
    }

    #endregion
}