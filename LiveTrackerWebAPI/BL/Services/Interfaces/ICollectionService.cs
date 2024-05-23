using BL.Models.Dto;
using BL.Models.ViewModels;

namespace BL.Services.Interfaces;

public interface ICollectionService
{
    Task<IList<CollectionBasicVM>> GetCollections();
    Task<CollectionDto> GetCollection(int collectionId);
    Task CreateCollection(CollectionDto collectionDto);
    Task UpdateCollection(CollectionDto collectionDto);
    Task RenameCollection(int collectionId, string collectionName);
    Task DeleteCollection(int collectionId);
}