using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers;

[TestFixture]
public class HomeControllerGetTests
{
    [Test, MoqAutoData]
    public void HomeIndex_ContainsExpectedModel(
        [Frozen] Mock<ISessionService> _sessionServiceMock,
        [Frozen] Mock<IOptions<ApplicationConfiguration>> mockOptions,
        [Frozen] ApplicationConfiguration configuration,
        [Greedy] HomeController sut)
    {
        var selectOrganisationLink = Guid.NewGuid().ToString();
        string addProviderUrl = Guid.NewGuid().ToString();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.SelectProvider, selectOrganisationLink)
            .AddUrlForRoute(RouteNames.AddProvider, addProviderUrl);

        mockOptions.Setup(c => c.Value).Returns(configuration);

        string allowedListUrl = new UriBuilder(configuration.AdminServicesBaseUrl) { Path = ExternalPaths.AdminServiceAllowedList }.Uri.ToString();

        var result = sut.Index() as ViewResult;
        result.Should().NotBeNull();
        var model = result.Model as ManageTrainingProviderViewModel;
        model!.AddANewTrainingProviderUrl.Should().Be(addProviderUrl);
        model.AddUkprnToAllowListUrl.Should().Be(allowedListUrl);
        model.SearchForTrainingProviderUrl.Should().Be(selectOrganisationLink);
        _sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddProvider), Times.Once());
    }

    [Test, MoqAutoData]
    public void Dashboard_ContainsExpectedConfig(
        [Frozen] Mock<ISessionService> _sessionServiceMock,
        [Frozen] Mock<IOptions<ApplicationConfiguration>> mockOptions,
        [Frozen] ApplicationConfiguration configuration,
        [Greedy] HomeController sut)
    {
        mockOptions.Setup(c => c.Value).Returns(configuration);

        var result = sut.Dashboard() as RedirectResult;
        result.Should().NotBeNull();
        result.Url.Should().Be(configuration.AdminServicesBaseUrl + "Dashboard");
    }
}
