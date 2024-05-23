using Newtonsoft.Json;

namespace BL.Models.Logic;

public class RecaptchaResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("score")]
    public decimal Score { get; set; }

    [JsonProperty("action")]
    public string? Action { get; set; }

    [JsonProperty("challenge_ts")]
    public string? ChallengeTs { get; set; }

    [JsonProperty("hostname")]
    public string? Hostname { get; set; }

    [JsonProperty("error-codes")]
    public object[]? ErrorCodes { get; set; }
}