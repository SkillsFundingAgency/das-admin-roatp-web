using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class OrganisationExtensionsTests
{
    [Test]
    public void WhenCheckingRestriction_AndCourseTypeIsRestricted_ThenReturnsTrue()
    {
        var organisation = CreateOrganisation(CourseType.Apprenticeship, isRestricted: true);

        organisation.IsRestrictedForCourseType(CourseType.Apprenticeship).Should().BeTrue();
    }

    [Test]
    public void WhenCheckingRestriction_AndCourseTypeIsNotRestricted_ThenReturnsFalse()
    {
        var organisation = CreateOrganisation(CourseType.Apprenticeship, isRestricted: false);

        organisation.IsRestrictedForCourseType(CourseType.Apprenticeship).Should().BeFalse();
    }

    [Test]
    public void WhenCheckingRestriction_AndCourseTypeIsMissing_ThenReturnsFalse()
    {
        var organisation = new GetOrganisationResponse { AllowedCourseTypes = [] };

        organisation.IsRestrictedForCourseType(CourseType.Apprenticeship).Should().BeFalse();
    }

    [Test]
    public void WhenCheckingIfUnrestricted_AndRouteIsApprenticeshipAndProviderIsNotRestricted_ThenReturnsTrue()
    {
        var organisation = CreateOrganisation(CourseType.Apprenticeship, isRestricted: false);

        organisation.IsUnrestrictedProvider(RouteLearningType.Apprenticeship).Should().BeTrue();
    }

    [Test]
    public void WhenCheckingIfUnrestricted_AndRouteIsApprenticeshipAndProviderIsRestricted_ThenReturnsFalse()
    {
        var organisation = CreateOrganisation(CourseType.Apprenticeship, isRestricted: true);

        organisation.IsUnrestrictedProvider(RouteLearningType.Apprenticeship).Should().BeFalse();
    }

    [Test]
    public void WhenCheckingIfUnrestricted_AndRouteIsApprenticeshipUnit_ThenReturnsFalse()
    {
        var organisation = CreateOrganisation(CourseType.ShortCourse, isRestricted: false);

        organisation.IsUnrestrictedProvider(RouteLearningType.ApprenticeshipUnit).Should().BeFalse();
    }

    private static GetOrganisationResponse CreateOrganisation(CourseType courseType, bool isRestricted)
        => new()
        {
            AllowedCourseTypes =
            [
                new AllowedCourseType
                {
                    CourseType = courseType,
                    IsRestricted = isRestricted
                }
            ]
        };
}
