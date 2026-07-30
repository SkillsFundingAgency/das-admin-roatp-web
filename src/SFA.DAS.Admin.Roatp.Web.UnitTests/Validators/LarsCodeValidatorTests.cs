using AutoFixture.NUnit4;
using FluentValidation.TestHelper;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class LarsCodeValidatorTests
{
    [Test, MoqAutoData]
    public async Task WhenValidatingLarsCode_AndServiceReturnsCourse_ThenIsValid(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LarsCodeValidator sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.TestValidateAsync(new LarsCodeModel { LarsCode = larsCode });

        result.ShouldNotHaveAnyValidationErrors();
        larsCodeServiceMock.Verify(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenValidatingLarsCode_AndServiceReturnsNull_ThenHasInvalidLarsCodeError(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LarsCodeValidator sut,
        string larsCode)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.TestValidateAsync(new LarsCodeModel { LarsCode = larsCode });

        result.ShouldHaveValidationErrorFor(x => x.LarsCode)
            .WithErrorMessage(LarsCodeValidator.LarsCodeInvalidValidationMessage);
    }

    [Test]
    [MoqInlineAutoData("")]
    [MoqInlineAutoData("   ")]
    public async Task WhenValidatingLarsCode_AndLarsCodeIsBlank_ThenHasEmptyErrorWithoutCallingService(
        string larsCode,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] LarsCodeValidator sut)
    {
        var result = await sut.TestValidateAsync(new LarsCodeModel { LarsCode = larsCode });

        result.ShouldHaveValidationErrorFor(x => x.LarsCode)
            .WithErrorMessage(LarsCodeValidator.LarsCodeEmptyValidationMessage);
        larsCodeServiceMock.Verify(s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
