using BL.Models.Dto;
using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = UserRole.Organizer)]
[Route("collections")]
public class CollectionsController : Controller
{
    #region Declaration 

    private readonly ICollectionService _collectionService;

    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    #endregion

    #region GET Methods

    [HttpGet]
    public async Task<IList<CollectionBasicVM>> GetCollections()
    {
        return await _collectionService.GetCollections();
    }

    [HttpGet("{collectionId}")]
    public async Task<CollectionDto> GetCollection(int collectionId)
    {
        return await _collectionService.GetCollection(collectionId);
    }

    #endregion

    #region POST Methods

    [HttpPost]
    public async Task InsertCollection([FromBody] CollectionDto collectionDto)
    {
        await _collectionService.CreateCollection(collectionDto);
    }

    #endregion

    #region PUT Methods

    [HttpPut]
    public async Task UpdateCollection([FromBody] CollectionDto collectionDto)
    {
        await _collectionService.UpdateCollection(collectionDto);
    }

    [HttpPut("rename")]
    public async Task RenameCollection([FromForm] int collectionId, [FromForm] string collectionName)
    {
        await _collectionService.RenameCollection(collectionId, collectionName);
    }

    [HttpDelete]
    public async Task DeleteCollection(int collectionId)
    {
        await _collectionService.DeleteCollection(collectionId);
    }

    #endregion
}