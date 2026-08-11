using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class RestrictedCourseDetailsControllerGetTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndLarsCodeIsValid_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = larsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Route = "Education and early years";
        response.LearningType = LearningType.Apprenticeship;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = 10019900,
                ProviderName = "BABINGTON LTD",
                LastDateStarts = DateTime.UtcNow.Date.AddDays(30)
            },
            new ProviderCourseModel
            {
                Ukprn = 10000001,
                ProviderName = "ACORN SKILLS TRAINING",
                LastDateStarts = null
            }
        ];

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, $"/restricted-courses/{larsCode}")
            .AddUrlForRoute(RouteNames.AddLastDateStarts, "/add-last-date-starts");

        var result = await sut.Index(larsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(RestrictedCourseDetailsController.ViewPath);
        var model = result.Model as RestrictedCourseDetailsViewModel;
        model.Should().NotBeNull();
        model!.DisplayTitle.Should().Be("Academic professional (Level 7)");
        model.Sector.Should().Be("Education and early years");
        model.LarsCode.Should().Be(larsCode);
        model.Level.Should().Be(7);
        model.StatusText.Should().Be("Restricted");
        model.HasProviders.Should().BeTrue();
        model.ProviderCount.Should().Be(2);
        model.ProviderCountDescription.Should().Be("2 providers");
        model.HasActiveFilters.Should().BeFalse();
        model.Filters.ShowFilterOptions.Should().BeFalse();
        model.Filters.FilterSections.Should().HaveCount(2);
        model.AllowedProviders.Select(p => p.ProviderName).Should().ContainInOrder("ACORN SKILLS TRAINING", "BABINGTON LTD");
        model.AllowedProviders.First().DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
        model.AllowedProviders.Last().DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
        model.AllowedProviders.Should().OnlyContain(p => p.ChangeUrl == "/add-last-date-starts");

        larsCodeServiceMock.Verify(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndNoProviders_ThenReturnsEmptyState(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = larsCode;
        response.IsCourseRestricted = true;
        response.Providers = [];

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, $"/restricted-courses/{larsCode}");

        var result = await sut.Index(larsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.HasNoProviders.Should().BeTrue();
        model.HasNoFilterResults.Should().BeFalse();
        model.AllowedProviders.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndTempDataContainsSuccessBannerMessage_ThenModelHasSuccessBannerMessage(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response,
        string successMessage)
    {
        response.LarsCode = larsCode;
        response.IsCourseRestricted = true;
        response.Providers = [];

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, $"/restricted-courses/{larsCode}");

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>())
        {
            [AddLastDateStartsController.SuccessBannerTempDataKey] = successMessage
        };

        var result = await sut.Index(larsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.AddLastDateStartsSuccessBannerMessage.Should().Be(successMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndLarsCodeIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] RestrictedCourseDetailsController sut,
        string larsCode)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.Index(larsCode, new GetRestrictedCourseDetailsRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()), Times.Once);
    }
}
