using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

public sealed class AllowedCoursesContent
{
    public required string CourseTypeName { get; init; }
    public required string CourseTypeNamePlural { get; init; }

    public string PageHeading => $"Manage {CourseTypeNamePlural} this provider is allowed to deliver";
    public string IntroText =>
        "This provider's account permissions mean that they can only offer courses we have added to their account.";
    public string AddHeading => $"Add an {CourseTypeName}";
    public string AddDescription =>
        $"Adding a new {CourseTypeName} will allow this provider to add it to their account in Course management.";
    public string AddSecondaryDescription =>
        "If they add it in Course management, they will be able to offer it to learners on Find apprenticeship training.";
    public string AddButtonText => $"Add an {CourseTypeName}";
    public string ListHeading => $"{char.ToUpperInvariant(CourseTypeNamePlural[0])}{CourseTypeNamePlural[1..]} this provider can deliver";
    public string ListIntro =>
        "This is a list of what courses this provider can add to their account in Course management and offer in Find apprenticeship training.";
    public string EmptyListText => $"There are currently no {CourseTypeNamePlural} added to this list.";

    public static AllowedCoursesContent CreateForCourseType(CourseType courseType)
        => courseType == CourseType.ShortCourse
            ? new()
            {
                CourseTypeName = "apprenticeship unit",
                CourseTypeNamePlural = "apprenticeship units"
            }
            : new()
            {
                CourseTypeName = "apprenticeship",
                CourseTypeNamePlural = "apprenticeships"
            };
}
