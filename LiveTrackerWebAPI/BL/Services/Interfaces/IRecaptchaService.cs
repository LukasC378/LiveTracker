namespace BL.Services.Interfaces
{
    public interface IRecaptchaService
    {
        /// <summary>
        /// Verify user by recaptcha
        /// </summary>
        /// <param name="token"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<bool> Verify(string token, string action);
    }
}
