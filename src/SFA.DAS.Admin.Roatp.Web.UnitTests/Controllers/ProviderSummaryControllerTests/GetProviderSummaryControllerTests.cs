using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderSummaryControllerTests;
public class GetProviderSummaryControllerTests
{
    [Test, MoqAutoData]
    public void Get_NoSessionModel_RedirectToHome(
        [Frozen] Mock<ISessionService> _sessionServiceMock,
        [Greedy] ProviderSummaryController sut)
    {
        _sessionServiceMock.Setup(x => x.Get<EditOrganisationSessionModel>(SessionKeys.EditOrganisation)).Returns((EditOrganisationSessionModel)null!);

        var actual = sut.Index();
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public void Get_Stuff(
        [Frozen] Mock<ISessionService> _sessionServiceMock,
        [Frozen] EditOrganisationSessionModel _editOrganisationSessionModel,
        [Greedy] ProviderSummaryController sut,
        string selectOrganisationLink)
    {
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectProvider, selectOrganisationLink);

        _sessionServiceMock.Setup(x => x.Get<EditOrganisationSessionModel>(SessionKeys.EditOrganisation)).Returns(_editOrganisationSessionModel);

        var actual = sut.Index() as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ProviderSummaryViewModel;
        model.Should().NotBeNull();
        model.Ukprn.Should().Be(_editOrganisationSessionModel.Ukprn);
        model.SearchProviderUrl.Should().Be(selectOrganisationLink);
    }
}
