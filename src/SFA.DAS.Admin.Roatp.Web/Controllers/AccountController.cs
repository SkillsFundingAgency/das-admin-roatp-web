using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("")]
public class AccountController : Controller
{
    [HttpGet]
    [Route("SignOut")]
    public new IActionResult SignOut()
    {
        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }

        return SignOut(
            new AuthenticationProperties(),
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    [Route("SignedOut")]
    public IActionResult SignedOut()
    {
        return View();
    }
}
