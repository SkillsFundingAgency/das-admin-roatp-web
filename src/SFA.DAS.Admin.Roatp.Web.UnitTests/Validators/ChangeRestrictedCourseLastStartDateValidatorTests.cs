using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class ChangeRestrictedCourseLastStartDateValidatorTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";

    [Test, MoqAutoData]
    public async Task WhenValidating_AndLarsCodeIsInRestrictedApprenticeships_ThenIsValid(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeRestrictedCourseLastStartDateValidator sut)
    {
        SetupRestrictedApprenticeships(outerApiClientMock, CreateCourse("99"), CreateCourse(LarsCode));

        var result = await sut.TestValidateAsync(CreateModel());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test, MoqAutoData]
    public async Task WhenValidating_AndLarsCodeIsNotInRestrictedApprenticeships_ThenIsInvalid(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeRestrictedCourseLastStartDateValidator sut)
    {
        SetupRestrictedApprenticeships(outerApiClientMock, CreateCourse("99"));

        var result = await sut.TestValidateAsync(CreateModel());

        result.ShouldHaveValidationErrorFor(model => model)
            .WithErrorMessage(ChangeRestrictedCourseLastStartDateValidator.CourseMustBeRestrictedForProviderErrorMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenValidating_AndRestrictedApprenticeshipsAreNotFound_ThenIsInvalid(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeRestrictedCourseLastStartDateValidator sut)
    {
        SetupRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.NotFound);

        var result = await sut.TestValidateAsync(CreateModel());

        result.ShouldHaveValidationErrorFor(model => model.Ukprn)
            .WithErrorMessage(ChangeRestrictedCourseLastStartDateValidator.ProviderMustBeUnrestrictedErrorMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenValidating_AndRestrictedApprenticeshipsReturnUnexpectedError_ThenIsInvalid(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeRestrictedCourseLastStartDateValidator sut)
    {
        SetupRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.InternalServerError);

        var result = await sut.TestValidateAsync(CreateModel());

        result.ShouldHaveValidationErrorFor(model => model.Ukprn)
            .WithErrorMessage(ChangeRestrictedCourseLastStartDateValidator.ProviderMustBeUnrestrictedErrorMessage);
    }

    private static ChangeRestrictedCourseLastStartDateModel CreateModel()
        => new()
        {
            Ukprn = Ukprn,
            LarsCode = LarsCode
        };

    private static ProviderRestrictedApprenticeshipModel CreateCourse(string larsCode)
        => new()
        {
            LarsCode = larsCode,
            Title = "Electrical",
            Level = 3
        };

    private static void SetupRestrictedApprenticeships(
        Mock<IOuterApiClient> outerApiClientMock,
        params ProviderRestrictedApprenticeshipModel[] courses)
        => SetupRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.OK, courses);

    private static void SetupRestrictedApprenticeships(
        Mock<IOuterApiClient> outerApiClientMock,
        HttpStatusCode statusCode,
        params ProviderRestrictedApprenticeshipModel[] courses)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.GetRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedApprenticeshipsResponse>(
                httpResponse,
                new GetRestrictedApprenticeshipsResponse { Courses = [.. courses] },
                new RefitSettings(),
                apiException));
    }
}
