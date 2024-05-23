using Newtonsoft.Json;

namespace BL.Models.Logic;

public class ErrorResponse
{
    [JsonProperty("message")]
    public string Message { get; set; }

    public ErrorResponse(Exception ex)
    {
        Message = ex.Message;
    }
}