using BL.Exceptions;
using BL.Models.Dto;
using BL.Services.Interfaces;
using DB.Database;
using DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace BL.Services;

public class LayoutsService : BaseService, ILayoutsService
{
    #region Declaration

    private readonly IUserService _userService;

    public LayoutsService(RaceTrackerDbContext dbContext, IUserService userService) : base(dbContext)
    {
        _userService = userService;
    }

    #endregion

    #region Public Methods

    public async Task<IEnumerable<LayoutDto>> GeyLayouts()
    {
        var userId = _userService.GetCurrentUserId();
        return await DbContext.Layout
            .Where(x => x.UserId == userId && x.Active)
            .Select(x => MapLayoutToDto(x))
            .ToListAsync();
    }

    public async Task<LayoutDto> GetLayout(int layoutId)
    {
        var userId = _userService.GetCurrentUserId();
        var layout = await GetLayoutWithAuthorization(layoutId, userId);
        return MapLayoutToDto(layout);
    }

    public async Task<int> CreateLayout(LayoutDto layoutDto)
    {
        var userId = _userService.GetCurrentUserId();
        var layout = await AddLayout(layoutDto, userId);

        await DbContext.SaveChangesAsync();

        return layout.Id;
    }

    public async Task UpdateLayout(LayoutDto layoutDto)
    {
        var userId = _userService.GetCurrentUserId();
        var layout = await GetLayoutWithAuthorization(layoutDto.Id, userId);
        var isUsed = await IsUsedLayout(layoutDto.Id);
        if (isUsed)
        {
            await AddLayout(layoutDto, userId);
            layout.Active = false;
        }
        else
        {
            layout.Name = layoutDto.Name;
            layout.GeoJson = layoutDto.GeoJson;
        }

        await DbContext.SaveChangesAsync();
    }

    public async Task RenameLayout(int layoutId, string layoutName)
    {
        var userId = _userService.GetCurrentUserId();
        var layout = await GetLayoutWithAuthorization(layoutId, userId);

        layout.Name = layoutName;

        await DbContext.SaveChangesAsync();
    }

    public async Task DeleteLayout(int layoutId)
    {
        var userId = _userService.GetCurrentUserId();
        var layout = await GetLayoutWithAuthorization(layoutId, userId);
        var isUsed = await IsUsedLayout(layoutId);
        if (isUsed)
        {
            layout.Active = false;
        }
        else
        {
            DbContext.Remove(layout);
        }

        await DbContext.SaveChangesAsync();
    }

    #endregion

    #region Private Methods

    private static LayoutDto MapLayoutToDto(Layout layout) =>
        new()
        {
            Id = layout.Id,
            GeoJson = layout.GeoJson,
            Name = layout.Name
        };

    private async Task<Layout> GetLayoutWithAuthorization(int layoutId, int userId)
    {
        var layout = await DbContext.Layout.FirstOrDefaultAsync(x => x.Id == layoutId);
        if (layout == null)
        {
            throw new ItemNotFoundException($"Layout {layoutId} not found");
        }
        if (layout.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }
        return layout;
    }

    private async Task<Layout> AddLayout(LayoutDto layoutDto, int userId)
    {
        var layout = new Layout
        {
            Active = true,
            GeoJson = layoutDto.GeoJson,
            Name = layoutDto.Name,
            UserId = userId
        };

        await DbContext.AddAsync(layout);

        return layout;
    }

    private async Task<bool> IsUsedLayout(int layoutId) =>
        await DbContext.Session.CountAsync(x => x.LayoutId == layoutId) > 0;


    #endregion
}