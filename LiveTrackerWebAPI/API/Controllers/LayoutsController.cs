using BL.Models.Dto;
using BL.Models.Logic;
using BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = UserRole.Organizer)]
[Route("layouts")]
public class LayoutsController : Controller
{
    #region Declaration

    private readonly ILayoutsService _layoutsService;

    public LayoutsController(ILayoutsService layoutsService)
    {
        _layoutsService = layoutsService;
    }

    #endregion

    #region GET Methods

    [HttpGet]
    public async Task<IEnumerable<LayoutDto>> GeyLayouts()
    {
        return await _layoutsService.GeyLayouts();
    }

    [HttpGet("{layoutId:int}")]
    public async Task<LayoutDto> GetLayout(int layoutId)
    {
        return await _layoutsService.GetLayout(layoutId);
    }

    #endregion

    #region POST Methods

    [HttpPost]
    public async Task<int> CreateLayout([FromBody] LayoutDto layout)
    {
        return await _layoutsService.CreateLayout(layout);
    }

    #endregion

    #region PUT Methods

    [HttpPut]
    public async Task UpdateLayout([FromBody] LayoutDto layout)
    {
        await _layoutsService.UpdateLayout(layout);
    }

    [HttpPut("rename")]
    public async Task RenameLayout([FromForm] int layoutId, [FromForm] string layoutName)
    {
        await _layoutsService.RenameLayout(layoutId, layoutName);
    }


    #endregion

    #region DELETE Methods

    [HttpDelete("{layoutId:int}")]
    public async Task DeleteLayout(int layoutId)
    {
        await _layoutsService.DeleteLayout(layoutId);
    }

    #endregion
}