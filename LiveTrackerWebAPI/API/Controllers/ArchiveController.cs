using BL.Models.ViewModels;
using BL.Services.Interfaces;
using LiveTrackerCommonModels.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("archive")]
public class ArchiveController : Controller
{
    #region Declaration

    private readonly IArchiveService _archiveService;

    public ArchiveController(IArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    #endregion

    #region GET Methods

    [HttpGet]
    public async Task<IEnumerable<RaceDataVM>> GetChunkFromArchive([FromQuery] int sessionId, [FromQuery] int start, [FromQuery] int length)
    {
        return await _archiveService.GetChunkFromArchive(sessionId, start, length);
    }

    [HttpGet("info/{sessionId:int}")]
    public async Task<ArchiveInfoVM> GetCountAndTime(int sessionId)
    {
        return await _archiveService.GetInfo(sessionId);
    }

    #endregion
}