using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

[TestFixture]
public class TempDataExtensionsTests
{
    [Test, MoqAutoData]
    public async Task GetProviderName_AndNameIsInTempData_ThenReturnsCachedNameWithoutCallingOrganisation(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        string providerName,
        int ukprn)
    {
        var sut = CreateTempData();
        sut[TempDataKeys.ProviderLegalName] = providerName;

        var result = await sut.GetProviderName(outerApiClientMock.Object, ukprn, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(providerName);
            sut.Peek(TempDataKeys.ProviderLegalName).Should().Be(providerName);
        }

        outerApiClientMock.Verify(c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task GetProviderName_AndNameIsNotInTempData_ThenLoadsNameFromOrganisationAndStoresIt(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse,
        int ukprn)
    {
        organisationResponse.Ukprn = ukprn;
        SetupOrganisation(outerApiClientMock, ukprn, organisationResponse, HttpStatusCode.OK);
        var sut = CreateTempData();

        var result = await sut.GetProviderName(outerApiClientMock.Object, ukprn, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(organisationResponse.LegalName);
            sut.Peek(TempDataKeys.ProviderLegalName).Should().Be(organisationResponse.LegalName);
        }

        outerApiClientMock.Verify(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetProviderName_AndOrganisationIsNotFound_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        GetOrganisationResponse organisationResponse,
        int ukprn)
    {
        SetupOrganisation(outerApiClientMock, ukprn, organisationResponse, HttpStatusCode.NotFound);
        var sut = CreateTempData();

        var result = await sut.GetProviderName(outerApiClientMock.Object, ukprn, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeNull();
            sut.Peek(TempDataKeys.ProviderLegalName).Should().BeNull();
        }
    }

    private static TempDataDictionary CreateTempData()
        => new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

    private static void SetupOrganisation(
        Mock<IOuterApiClient> outerApiClientMock,
        int ukprn,
        GetOrganisationResponse response,
        HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(statusCode),
                response,
                new RefitSettings(),
                null));
    }
}
