using BL.Models.Logic;
using BL.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BL.Services;

public class RecaptchaService : IRecaptchaService
{
    #region Declaration

    private readonly string _recaptchaSecretKey;
    private readonly IHttpClientFactory _httpClientFactory;

    public RecaptchaService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _recaptchaSecretKey = configuration["RecaptchaSecretKey"]!;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Verify user by recaptcha
    /// </summary>
    /// <param name="token"></param>
    /// <param name="action"></param>
    /// <returns></returns> 
    public async Task<bool> Verify(string token, string action)
    {
        using var client = _httpClientFactory.CreateClient(Constants.RECAPTCHA_CLIENT);

        var response = await client.PostAsync($"siteverify?secret={_recaptchaSecretKey}&response={token}", null);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var recaptchaResponse = JsonConvert.DeserializeObject<RecaptchaResponse>(responseBody);
        var success = recaptchaResponse?.Success ?? false;
        var actionEquals = recaptchaResponse?.Action == action;
        return success && actionEquals && recaptchaResponse?.Score > 0.5M;
    }

    #endregion
}