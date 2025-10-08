using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests;

[TestFixture]
public class HomeControllerGetTests
{
    [Test, MoqAutoData]
    public void HomeIndex_ContainsExpectedModel(
        [Greedy] HomeController sut)
    {
        var result = sut.Index() as ViewResult;
        result.Should().NotBeNull();
        result.Model.Should().BeEquivalentTo(new ManageTrainingProviderViewModel());
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
