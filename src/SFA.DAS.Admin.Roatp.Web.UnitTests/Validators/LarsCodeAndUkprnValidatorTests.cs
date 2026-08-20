using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class LarsCodeAndUkprnValidatorTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";

    [Test, MoqAutoData]
    public async Task IsValidAsync_WhenLarsCodeAndUkprnAreValid_ThenReturnsTrue(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] LarsCodeAndUkprnValidator sut,
        GetRestrictedCourseDetailsResponse courseDetails,
        GetOrganisationResponse organisation)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDetails);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var result = await sut.IsValidAsync(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public async Task IsValidAsync_WhenLarsCodeIsInvalid_ThenReturnsFalse(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] LarsCodeAndUkprnValidator sut)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.IsValidAsync(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeFalse();
        ukprnServiceMock.Verify(
            s => s.GetOrganisationAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task IsValidAsync_WhenUkprnIsInvalid_ThenReturnsFalse(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] LarsCodeAndUkprnValidator sut,
        GetRestrictedCourseDetailsResponse courseDetails)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDetails);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.IsValidAsync(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeFalse();
    }
}
