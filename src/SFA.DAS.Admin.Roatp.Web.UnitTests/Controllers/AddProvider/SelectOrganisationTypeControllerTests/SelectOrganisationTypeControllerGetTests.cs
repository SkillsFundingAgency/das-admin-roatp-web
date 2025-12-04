using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectOrganisationTypeControllerTests;
public class SelectOrganisationTypeControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_Index_ReturnsView(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationTypesService> organisationTypesServiceMock,
        [Greedy] SelectOrganisationTypeController sut,
        List<OrganisationTypeModel> organisationTypes,
        AddProviderSessionModel sessionModel)
    {
        //Arrange
        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);
        organisationTypesServiceMock.Setup(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()))!
            .ReturnsAsync(organisationTypes);

        // Act
        var result = await sut.Index(CancellationToken.None) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result!.Model.Should().BeOfType<OrganisationTypeViewModel>();
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        organisationTypesServiceMock.Verify(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()), Times.Once());
    }
}
