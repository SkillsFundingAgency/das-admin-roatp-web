using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresSessionModelAttribute<T>(string sessionKey, string redirectToRouteName) : Attribute, IAuthorizationFilter where T : class, ISessionModel
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var sessionService = context.HttpContext.RequestServices.GetRequiredService<ISessionService>();
        T? sessionModel = sessionService.Get<T>(sessionKey);
        if (sessionModel == null)
        {
            context.Result = new RedirectToRouteResult(redirectToRouteName, null);
        }
    }
}
