using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.RegisteredProvidersControllerTests;
public class GetRegisteredProvidersControllerTests
{
    private const string SearchTerm = "test";
    private const string UkprnSearchString = "100";
    private const int UkprnSeed = 1000000;

    [Test, MoqAutoData]
    public async Task GetOrganisations_MatchedByName(
        [Frozen] Mock<IOrganisationService> organisationServiceMock,
        [Greedy] RegisteredProvidersController controller,
        List<OrganisationModel> organisations,
        CancellationToken cancellationToken
    )
    {
        foreach (var organisation in organisations)
        {
            organisation.LegalName = SearchTerm + organisation.LegalName;
        }

        organisationServiceMock.Setup(x => x.GetOrganisations(It.IsAny<CancellationToken>())).ReturnsAsync(organisations);

        var actual = await controller.Index(SearchTerm, cancellationToken);
        var actualResult = actual as OkObjectResult;
        var returnedOrganisations = new List<OrganisationModel>((IEnumerable<OrganisationModel>)actualResult!.Value!);

        actual.Should().NotBeNull();
        actualResult.Should().NotBeNull();
        returnedOrganisations.Should().BeEquivalentTo(organisations);
        organisationServiceMock.Verify(x => x.GetOrganisations(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetOrganisations_MatchedByUkprn(
        [Frozen] Mock<IOrganisationService> organisationServiceMock,
        [Greedy] RegisteredProvidersController controller,
        CancellationToken cancellationToken
    )
    {
        int numberOfMockedRegisteredProviders = 10;
        var registeredProviders = new List<OrganisationModel>();

        for (var i = 0; i < numberOfMockedRegisteredProviders; i++)
        {
            registeredProviders.Add(new OrganisationModel { LegalName = Guid.NewGuid().ToString(), Ukprn = UkprnSeed + i });
        }


        organisationServiceMock.Setup(x => x.GetOrganisations(It.IsAny<CancellationToken>())).ReturnsAsync(registeredProviders);

        var actual = await controller.Index(UkprnSearchString, cancellationToken);
        var actualResult = actual as OkObjectResult;
        var returnedOrganisations = new List<OrganisationModel>((IEnumerable<OrganisationModel>)actualResult!.Value!);

        actual.Should().NotBeNull();
        actualResult.Should().NotBeNull();
        returnedOrganisations.Should().BeEquivalentTo(registeredProviders);
        organisationServiceMock.Verify(x => x.GetOrganisations(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Test, MoqAutoData]
    public async Task GetOrganisations_ReturnedTop100MatchesSortedAlphabetically(
        [Frozen] Mock<IOrganisationService> organisationServiceMock,
        [Greedy] RegisteredProvidersController controller,
        CancellationToken cancellationToken
    )
    {
        int numberOfMockedRegisteredProviders = 200;
        int numberOfExpectedRegisteredProviders = 100;
        var mockedRegisteredProviders = new List<OrganisationModel>();

        for (var i = 0; i < numberOfMockedRegisteredProviders; i++)
        {
            mockedRegisteredProviders.Add(new OrganisationModel() { LegalName = Guid.NewGuid().ToString(), Ukprn = UkprnSeed + i });
        }

        organisationServiceMock.Setup(x => x.GetOrganisations(It.IsAny<CancellationToken>())).ReturnsAsync(mockedRegisteredProviders);


        var actual = await controller.Index(UkprnSearchString, cancellationToken);
        var actualResult = actual as OkObjectResult;
        var returnedProviders = new List<OrganisationModel>((IEnumerable<OrganisationModel>)actualResult!.Value!);

        actual.Should().NotBeNull();
        actualResult.Should().NotBeNull();
        returnedProviders.Count.Should().Be(numberOfExpectedRegisteredProviders);
        returnedProviders.Should().BeEquivalentTo(mockedRegisteredProviders
            .OrderBy(x => x.LegalName)
            .Take(numberOfExpectedRegisteredProviders));

    }

    [TestCase(null)]
    [TestCase("a")]
    [TestCase("ab")]
    [TestCase(" AB ")]
    [TestCase("")]
    public async Task GetOrganisations_LessThan3Characters_NoResultsExpected(string searchTerm)
    {
        Mock<IOrganisationService> organisationServiceMock = new Mock<IOrganisationService>();

        var controller = new RegisteredProvidersController(organisationServiceMock.Object);

        var actual = await controller.Index(searchTerm!, new CancellationToken());
        var actualResult = actual as OkObjectResult;
        var returnedProviders = new List<OrganisationModel>((IEnumerable<OrganisationModel>)actualResult!.Value!);

        actual.Should().NotBeNull();
        actualResult.Should().NotBeNull();
        returnedProviders.Count.Should().Be(0);
        organisationServiceMock.Verify(x => x.GetOrganisations(It.IsAny<CancellationToken>()), Times.Never);
    }
}
