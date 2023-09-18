using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace VSOnline.VSECommerce.Controllers
{
    [ApiController]
    [Route("/api/[controller]/[action]/{id?}")]
    public abstract partial class VSControllerBase : Controller
    {
        protected internal int STATUSCODE_FAILURE = 417;
        protected internal int STATUSCODE_ERROR = 500;

        protected internal readonly AppSettings _appSettings;
        protected internal readonly ILogger<VSControllerBase> _logger;


        protected internal VSControllerBase(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        protected internal Guid CurrentUserGuid
        {
            get
            {
                if (HttpContext.User.HasClaim(c => c.Type == "jti"))
                {
                    Guid.TryParse(HttpContext.User.FindFirst("jti").Value, out var result);
                    return result;
                }
                return Guid.Empty;
            }
        }

        protected internal int CurrentUserId
        {
            get
            {
                if (HttpContext.User.HasClaim(c => c.Type == "jti"))
                {
                    //Guid.TryParse(HttpContext.User.FindFirst("jti").Value, out var result);
                    return Convert.ToInt32(HttpContext.User.FindFirst("jti").Value);
                }
                return 0;
            }
        }

        protected internal string CurrentUsername()
        {
            if (HttpContext.User.HasClaim(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"))
            {
                return HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            }
            return string.Empty;
        }

    }
}
