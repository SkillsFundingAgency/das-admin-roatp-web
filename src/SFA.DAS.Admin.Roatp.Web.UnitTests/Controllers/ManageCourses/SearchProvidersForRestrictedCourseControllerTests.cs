using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class SearchProvidersForRestrictedCourseControllerTests
{
    private const string LarsCode = "105";

    [Test, MoqAutoData]
    public async Task WhenSearching_AndProvidersMatch_ThenReturnsMatchingNotAllowedProviders(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] SearchProvidersForRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidLarsCode(larsCodeServiceMock, response);
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 10007938, ProviderName = "BP TRAINING", LastDateStarts = null },
            new ProviderCourseModel { Ukprn = 10000002, ProviderName = "OTHER PROVIDER", LastDateStarts = null }
        ];
        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(LarsCode, "BP", CancellationToken.None) as OkObjectResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            var providers = result!.Value as IEnumerable<OrganisationModel>;
            providers.Should().NotBeNull();
            providers!.Select(p => p.Ukprn).Should().ContainSingle().Which.Should().Be(10007938);
            providers.Single().LegalName.Should().Be("BP TRAINING");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenSearching_AndQueryIsEmpty_ThenReturnsEmptyList(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] SearchProvidersForRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidLarsCode(larsCodeServiceMock, response);

        var result = await sut.Index(LarsCode, " ", CancellationToken.None) as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().BeAssignableTo<IEnumerable<OrganisationModel>>()
            .Which.Should().BeEmpty();
        notAllowedProvidersServiceMock.Verify(
            s => s.GetNotAllowedProvidersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenSearching_AndLarsCodeIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] SearchProvidersForRestrictedCourseController sut)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.Index(LarsCode, "BP", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        notAllowedProvidersServiceMock.Verify(
            s => s.GetNotAllowedProvidersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenSearching_AndCourseIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] SearchProvidersForRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidLarsCode(larsCodeServiceMock, response);
        response.IsCourseRestricted = false;
        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(LarsCode, "BP", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenSearching_AndApiReturnsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] SearchProvidersForRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidLarsCode(larsCodeServiceMock, response);
        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.Index(LarsCode, "BP", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupValidLarsCode(
        Mock<ILarsCodeService> larsCodeServiceMock,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}
