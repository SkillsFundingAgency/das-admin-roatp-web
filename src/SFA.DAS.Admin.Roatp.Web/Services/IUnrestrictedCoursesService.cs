using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IUnrestrictedCoursesService
{
    Task<List<RestrictedCourseModel>> GetUnrestrictedCourses(CancellationToken cancellationToken);
}
