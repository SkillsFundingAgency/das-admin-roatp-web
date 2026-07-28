using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class LarsCodeValidatorTests
{
    [Test, MoqAutoData]
    public async Task WhenValidatingLarsCode_AndApiReturnsCourse_ThenIsValid(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LarsCodeValidator sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var result = await sut.TestValidateAsync(larsCode);

        result.ShouldNotHaveAnyValidationErrors();
        outerApiClientMock.Verify(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsCourse_ThenReturnsContent(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LarsCodeValidator sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var result = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        result.Should().Be(response);
    }

    [Test, MoqAutoData]
    public async Task WhenValidatingLarsCode_AndApiFails_ThenHasValidationError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LarsCodeValidator sut,
        string larsCode)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.TestValidateAsync(larsCode);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(LarsCodeValidator.LarsCodeValidationMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiFails_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LarsCodeValidator sut,
        string larsCode)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingCourseDetails_AndApiReturnsOkWithNullContent_ThenReturnsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LarsCodeValidator sut,
        string larsCode)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), null, new RefitSettings(), null));

        var result = await sut.GetCourseDetailsAsync(larsCode, CancellationToken.None);

        result.Should().BeNull();
    }

    [Test]
    [MoqInlineAutoData(null)]
    [MoqInlineAutoData("")]
    [MoqInlineAutoData("   ")]
    public async Task WhenGettingCourseDetails_AndLarsCodeIsBlank_ThenReturnsNullWithoutCallingApi(
        string? larsCode,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LarsCodeValidator sut)
    {
        var result = await sut.GetCourseDetailsAsync(larsCode!, CancellationToken.None);

        result.Should().BeNull();
        outerApiClientMock.Verify(c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    [MoqInlineAutoData("")]
    [MoqInlineAutoData("   ")]
    public async Task WhenValidatingLarsCode_AndLarsCodeIsBlank_ThenHasValidationErrorWithoutCallingApi(
        string larsCode,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] LarsCodeValidator sut)
    {
        var result = await sut.TestValidateAsync(larsCode);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(LarsCodeValidator.LarsCodeValidationMessage);
        outerApiClientMock.Verify(c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
