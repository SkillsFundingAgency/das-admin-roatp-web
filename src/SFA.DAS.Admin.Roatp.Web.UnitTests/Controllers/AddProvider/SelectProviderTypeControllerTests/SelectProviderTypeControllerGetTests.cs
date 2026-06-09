using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Constants;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectProviderTypeControllerTests;
public class SelectProviderTypeControllerGetTests
{
    [Test]
    [MoqInlineAutoData(null, 0)]
    [MoqInlineAutoData(1, 1)]
    public void Get_Index_SessionsReturnsVariationOfProviderTypeId_ReturnsViewModel(
        int? ProviderTypeId,
        int expectedproviderTypeId,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectProviderTypeController sut)
    {
        // Arrange
        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = 12345678,
            LegalName = "LegalName",
            TradingName = "TradingName",
            CompanyNumber = "12345678",
            CharityNumber = "12345678",
            ProviderTypeId = ProviderTypeId
        };

        sessionServiceMock.Setup(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider)).Returns(sessionModel);

        var providerTypes = BuildProviderTypes(expectedproviderTypeId);

        // Act
        var result = sut.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result!.Model.Should().BeOfType<SelectProviderTypeViewModel>();
        var model = result!.Model as SelectProviderTypeViewModel;
        model!.SelectedProviderTypeId.Should().Be(expectedproviderTypeId);
        model!.ProviderTypes.Should().BeEquivalentTo(providerTypes);
        sessionServiceMock.Verify(s => s.Get<AddProviderSessionModel>(SessionKeys.AddProvider), Times.Once());
    }

    private static List<AddProviderTypeSelectionModel> BuildProviderTypes(int providerTypeId)
    {
        return new List<AddProviderTypeSelectionModel>
        {
            new() { Description = ProviderTypeDescription.Main, Id = (int)ProviderType.Main, IsSelected = providerTypeId == (int)ProviderType.Main },
            new() { Description = ProviderTypeDescription.Employer, Id = (int)ProviderType.Employer, IsSelected = providerTypeId == (int)ProviderType.Employer },
            new() { Description = ProviderTypeDescription.Supporting, Id = (int)ProviderType.Supporting, IsSelected = providerTypeId == (int)ProviderType.Supporting },
        };
    }
}
