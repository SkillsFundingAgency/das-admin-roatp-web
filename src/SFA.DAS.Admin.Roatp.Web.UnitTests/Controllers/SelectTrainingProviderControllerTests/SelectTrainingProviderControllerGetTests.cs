using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.SelectTrainingProviderControllerTests;
public class SelectTrainingProviderControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_BuildsViewModel(
        [Frozen] ISessionService _sessionService,
        [Frozen] IValidator<SelectTrainingProviderViewModel> _validator,
        [Greedy] SelectTrainingProviderController controller)
    {
        var actual = controller.Index() as ViewResult;

        actual.Should().NotBeNull();
        var model = actual!.Model as SelectTrainingProviderViewModel;
        model.Should().NotBeNull();
    }
}
