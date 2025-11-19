using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AddProvider.SelectProviderControllerTests;
public class SelectProviderControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_Index_ReturnsView(
        [Greedy] SelectProviderController sut)
    {
        // Act
        var result = sut.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result!.Model.Should().BeOfType<AddProviderViewModel>();
    }

    [Test, MoqAutoData]
    public void Get_ProviderNotFoundInUkrlp_ReturnsView(
    [Greedy] SelectProviderController sut)
    {
        // Act
        var result = sut.ProviderNotFoundInUkrlp() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result!.Model.Should().BeOfType<AddProviderViewModel>();
    }
}