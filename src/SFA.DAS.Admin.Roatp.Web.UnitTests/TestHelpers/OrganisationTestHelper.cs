using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;

public static class OrganisationTestHelper
{
    public static void SetupOrganisationRestriction(
        this Mock<IUkprnService> ukprnServiceMock,
        int ukprn,
        bool isRestricted,
        CourseType courseType = CourseType.Apprenticeship)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrganisationResponse
            {
                Ukprn = ukprn,
                AllowedCourseTypes =
                [
                    new AllowedCourseType
                    {
                        CourseType = courseType,
                        IsRestricted = isRestricted
                    }
                ]
            });
    }
}
