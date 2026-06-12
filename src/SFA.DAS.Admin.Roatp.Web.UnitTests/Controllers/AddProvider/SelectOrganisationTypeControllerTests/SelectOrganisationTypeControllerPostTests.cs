using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
public class SelectOrganisationTypeControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Post_Index_SubmitModelIsValid_SetsSessionAndRedirectsToCorrectAction(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationTypesService> organisationTypesServiceMock,
        [Frozen] Mock<IValidator<OrganisationTypeSubmitModel>> validator,
        [Greedy] SelectOrganisationTypeController sut,
        AddProviderSessionModel sessionModel)
    {
        //Arrange
        OrganisationTypeSubmitModel submitModel = new() { OrganisationTypeId = 1 };
        var organisationTypes = new List<OrganisationTypeModel>
        {
            new OrganisationTypeModel
            {
                Id = 1,
                Description = "Test"
            }
        };

        validator.Setup(x => x.Validate(It.Is<OrganisationTypeSubmitModel>(m => m.OrganisationTypeId == submitModel.OrganisationTypeId))).Returns(new ValidationResult());
        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);
        organisationTypesServiceMock.Setup(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()))!
            .ReturnsAsync(organisationTypes);

        // Act
        var result = await sut.Index(submitModel, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var redirectResult = result as RedirectToRouteResult;
        redirectResult.Should().NotBeNull();
        redirectResult.RouteName.Should().Be(RouteNames.ProviderDetailsSummary);
        sessionModel.OrganisationTypeId.Should().Be(submitModel.OrganisationTypeId);
        sessionModel.OrganisationType.Should().Be("Test");
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OrganisationTypeId == sessionModel.OrganisationTypeId)), Times.Once);
        organisationTypesServiceMock.Verify(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test, MoqAutoData]
    public async Task Post_Index_NotMatchingOrganiationType_SetOrganisationTypeToNull(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationTypesService> organisationTypesServiceMock,
        [Frozen] Mock<IValidator<OrganisationTypeSubmitModel>> validator,
        [Greedy] SelectOrganisationTypeController sut,
        AddProviderSessionModel sessionModel)
    {
        //Arrange
        OrganisationTypeSubmitModel submitModel = new() { OrganisationTypeId = 1 };
        var organisationTypes = new List<OrganisationTypeModel>();

        validator.Setup(x => x.Validate(It.Is<OrganisationTypeSubmitModel>(m => m.OrganisationTypeId == submitModel.OrganisationTypeId))).Returns(new ValidationResult());
        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);
        organisationTypesServiceMock.Setup(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()))!
            .ReturnsAsync(organisationTypes);

        // Act
        await sut.Index(submitModel, CancellationToken.None);

        // Assert
        sessionModel.OrganisationType.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task Post_Index_SubmitModelIsInvalid_ReturnsViewWithErrors(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationTypesService> organisationTypesServiceMock,
        [Frozen] Mock<IValidator<OrganisationTypeSubmitModel>> validator,
        [Greedy] SelectOrganisationTypeController sut,
        List<OrganisationTypeModel> organisationTypes,
        AddProviderSessionModel sessionModel)
    {
        //Arrange
        OrganisationTypeSubmitModel submitModel = new();

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));

        validator.Setup(x => x.Validate(It.Is<OrganisationTypeSubmitModel>(m => m == submitModel))).Returns(validationResult);

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);
        organisationTypesServiceMock.Setup(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()))!
            .ReturnsAsync(organisationTypes);

        // Act
        var result = await sut.Index(submitModel, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<OrganisationTypeViewModel>();
        sut.ModelState.ErrorCount.Should().Be(1);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AddProvider, It.Is<AddProviderSessionModel>(m =>
            m.OrganisationTypeId == sessionModel.OrganisationTypeId)), Times.Never);
        organisationTypesServiceMock.Verify(x => x.GetOrganisationTypes(It.IsAny<CancellationToken>()), Times.Once());
    }
}
