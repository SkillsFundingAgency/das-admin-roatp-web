using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers;

[TestFixture]
public class HomeControllerGetTests
{
    [Test, MoqAutoData]
    public void HomeIndex_ContainsExpectedModel(
        [Greedy] HomeController sut)
    {
        var selectOrganisationLink = Guid.NewGuid().ToString();
        sut.AddDefaultContextWithUser().AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectProvider, selectOrganisationLink);

        var result = sut.Index() as ViewResult;
        result.Should().NotBeNull();
        var model = result.Model as ManageTrainingProviderViewModel;
        model!.AddANewTrainingProviderUrl.Should().Be("#");
        model.AddUkprnToAllowListUrl.Should().Be("#");
        model.SearchForTrainingProviderUrl.Should().Be(selectOrganisationLink);
    }

    [Test, MoqAutoData]
    public void Dashboard_ContainsExpectedConfig(
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
