using Microsoft.AspNetCore.SignalR;

namespace LiveTrackerSessionAPI.Hubs;

public class RaceHub : Hub
{
    public async Task JoinRace(string raceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, raceId);
    }

    public async Task LeaveRace(string raceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, raceId);
    }
}