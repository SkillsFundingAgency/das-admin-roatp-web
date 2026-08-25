using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class UkprnAndLarsCodeValidatorTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";

    [Test, MoqAutoData]
    public async Task WhenUkprnAndLarsCodeAreValid_ThenIsValid(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        GetRestrictedCourseDetailsResponse courseDetails,
        GetOrganisationResponse organisation)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDetails);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var sut = CreateSut(larsCodeServiceMock, ukprnServiceMock);
        var result = await sut.TestValidateAsync(new UkprnAndLarsCodeModel { Ukprn = Ukprn, LarsCode = LarsCode });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test, MoqAutoData]
    public async Task WhenLarsCodeIsInvalid_ThenHasError(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        GetOrganisationResponse organisation)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var sut = CreateSut(larsCodeServiceMock, ukprnServiceMock);
        var result = await sut.TestValidateAsync(new UkprnAndLarsCodeModel { Ukprn = Ukprn, LarsCode = LarsCode });

        result.ShouldHaveValidationErrorFor(x => x.LarsCode)
            .WithErrorMessage(LarsCodeValidator.LarsCodeInvalidValidationMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenUkprnIsInvalid_ThenHasError(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        GetRestrictedCourseDetailsResponse courseDetails)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDetails);
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var sut = CreateSut(larsCodeServiceMock, ukprnServiceMock);
        var result = await sut.TestValidateAsync(new UkprnAndLarsCodeModel { Ukprn = Ukprn, LarsCode = LarsCode });

        result.ShouldHaveValidationErrorFor(x => x.Ukprn)
            .WithErrorMessage(UkprnValidator.UkprnInvalidValidationMessage);
    }

    private static UkprnAndLarsCodeValidator CreateSut(
        Mock<ILarsCodeService> larsCodeServiceMock,
        Mock<IUkprnService> ukprnServiceMock)
        => new(
            new UkprnValidator(ukprnServiceMock.Object),
            new LarsCodeValidator(larsCodeServiceMock.Object));
}
