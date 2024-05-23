using idunno.Authentication.Basic;
using LiveTrackerSessionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SessionAPICommonModels;
using SessionAPICommonModels.Dtos;

namespace LiveTrackerSessionAPI.Controllers;

[Route("race")]
public class RaceController : ControllerBase
{
    private readonly ITrackService _trackService;

    public RaceController(ITrackService trackService)
    {
        _trackService = trackService;
    }

    //[Authorize(Policy = JwtBearerDefaults.AuthenticationScheme)]
    //[HttpGet("positions/{raceId:int}")]
    //public ActionResult<List<GpsData>> GetPositions(int raceId)
    //{
    //    return _trackService.DetermineDriversOrder(raceId);
    //}

    [Authorize(Policy = BasicAuthenticationDefaults.AuthenticationScheme)]
    [HttpPost("load")]
    public void LoadRaceData([FromBody] RaceDataDto raceData)
    {
        _trackService.LoadRaceData(raceData);
    }


    [Authorize(Policy = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("update")]
    public async Task UpdateGpsData([FromBody] IList<GpsData> gpsData)
    {
        await _trackService.UpdateGpsData(gpsData);
    }

    [Authorize(Policy = BasicAuthenticationDefaults.AuthenticationScheme)]
    [HttpDelete("unload/{raceId:int}")]
    public IEnumerable<string>? UnloadRaceData(int raceId)
    {
        return _trackService.UnloadRaceData(raceId);
    }
}