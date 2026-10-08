using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface ICoursesService
{
    Task<GetCourseResponse?> GetCourse(string larsCode, CancellationToken cancellationToken);
}
