using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class OrganisationExtensions
{
    public static bool IsRestrictedForCourseType(this GetOrganisationResponse organisation, CourseType courseType)
        => organisation.AllowedCourseTypes.FirstOrDefault(x => x.CourseType == courseType)?.IsRestricted == true;
}
