using BL.Models.Dto;

namespace BL.Services.Interfaces;

public interface ILayoutsService
{
    Task<IEnumerable<LayoutDto>> GeyLayouts();
    Task<LayoutDto> GetLayout(int layoutId);
    Task<int> CreateLayout(LayoutDto layoutDto);
    Task UpdateLayout(LayoutDto layoutDto);
    Task RenameLayout(int layoutId, string layoutName);
    Task DeleteLayout(int layoutId);
}