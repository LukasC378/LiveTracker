using BL.Models.Dto;
using BL.Models.Logic;
using BL.Models.ViewModels;
using static BL.Models.ViewModels.SessionsGroupVM;

namespace BL.Services.Interfaces;

public interface ISessionService
{
    Task<SessionToEditDto> GetSessionToEdit(int sessionId);
    Task<SessionVM> GetSession(int sessionId);
    Task<IEnumerable<SessionToManageVM>> GetSessionsToManage();
    Task<IEnumerable<SessionsGroupVM>> GetLiveSessions();
    Task<IEnumerable<SessionsGroupVM>> GetFilteredSessions(SessionFilter filter);
    Task<SessionStateEnum> GetSessionState(int sessionId);
    Task<SessionResultVM> GetSessionResult(int sessionId);
    Task<IEnumerable<SessionToLoad>> GetSessionsToLoad();
    Task<IEnumerable<SessionToUnload>> GetSessionsToUnload();
    Task CreateSession(SessionDto sessionDto);
    Task UpdateSession(SessionDto sessionDto);
    Task UpdateLoadedSessions(IEnumerable<int> sessionIds);
    Task UpdateEndedSessions(List<(int RaceId, IEnumerable<Guid> Result)> results);
    Task CancelSession(int sessionId);
}