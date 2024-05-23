using BL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("recaptcha")]
public class RecaptchaController : Controller
{
    #region Declaration

    private readonly IRecaptchaService _recaptchaService;

    public RecaptchaController(IRecaptchaService recaptchaService)
    {
        _recaptchaService = recaptchaService;
    }

    #endregion

    #region GET Methods

    /// <summary>
    /// Returns true if user is not bot
    /// </summary>
    /// <param name="token"></param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<bool> RecaptchaVerification([FromQuery] string token, [FromQuery] string actionType)
    {
        return await _recaptchaService.Verify(token, actionType);
    }

    #endregion
}