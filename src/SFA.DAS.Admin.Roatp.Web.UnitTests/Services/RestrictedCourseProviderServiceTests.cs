using System.Security.Claims;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class RestrictedCourseProviderServiceTests
{
    [Test, MoqAutoData]
    public async Task IsRouteValidAsync_WhenValidationPasses_ThenReturnsTrue(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        RestrictedCourseProviderService sut)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var result = await sut.IsRouteValidAsync("105", 10007938, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public async Task GetCourseAndProviderAsync_WhenProviderExists_ThenReturnsCourseAndProvider(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        RestrictedCourseProviderService sut,
        GetRestrictedCourseDetailsResponse response)
    {
        const int ukprn = 10007938;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = ukprn, ProviderName = "BP TRAINING" }
        ];
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync("105", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.GetCourseAndProviderAsync("105", ukprn, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Value.Course.Should().Be(response);
            result.Value.Provider.Ukprn.Should().Be(ukprn);
        }
    }

    [Test, MoqAutoData]
    public void CreateUpsertRequest_ThenMapsUserAndLastDateStarts(RestrictedCourseProviderService sut)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", "test.user@education.gov.uk")
        ], "test"));
        var lastDateStarts = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);

        var result = sut.CreateUpsertRequest(user, lastDateStarts);

        using (new AssertionScope())
        {
            result.UserId.Should().Be("test.user@education.gov.uk");
            result.UserDisplayName.Should().Be("Test User");
            result.LastDateStarts.Should().Be(lastDateStarts);
        }
    }
}
