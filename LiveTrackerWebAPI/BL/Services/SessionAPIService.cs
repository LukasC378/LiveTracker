using System.Text;
using BL.Services.Interfaces;
using LiveTrackerCommonModels.Dtos;
using LiveTrackerModels;
using Newtonsoft.Json;

namespace BL.Services;

public class SessionApiService : ISessionApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SessionApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetSessionToken(TokenInput tokenInput)
    {
        using var client = _httpClientFactory.CreateClient(Constants.SESSION_API_CLIENT);

        var content = new StringContent(JsonConvert.SerializeObject(tokenInput), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("token/generate", content);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadAsStringAsync();

        return token;
    }

    public async Task LoadRace(RaceDataDto raceDataDto)
    {
        using var client = _httpClientFactory.CreateClient(Constants.SESSION_API_CLIENT);

        var content = new StringContent(JsonConvert.SerializeObject(raceDataDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("race/load", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<Guid>?> UnloadRace(int raceId)
    {
        using var client = _httpClientFactory.CreateClient(Constants.SESSION_API_CLIENT);

        var response = await client.DeleteAsync("race/unload/" + raceId);
        response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<IEnumerable<Guid>?>(resultJson);

        return result;
    }
}