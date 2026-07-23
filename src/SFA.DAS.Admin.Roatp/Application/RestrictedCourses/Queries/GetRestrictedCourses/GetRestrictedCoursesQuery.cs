using MediatR;
using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Application.RestrictedCourses.Queries.GetRestrictedCourses;

public class GetRestrictedCoursesQuery : IRequest<GetRestrictedCoursesQueryResult>
{
    public bool Restricted { get; set; } = true;
    public string? CourseName { get; set; }
    public List<LearningType> LearningTypes { get; set; } = [];
}
