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
public class UkprnValidatorTests
{
    [Test, MoqAutoData]
    public async Task WhenValidatingUkprn_AndServiceReturnsOrganisation_ThenIsValid(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] UkprnValidator sut,
        GetOrganisationResponse organisation)
    {
        const int ukprn = 10007938;
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);

        var result = await sut.TestValidateAsync(new UkprnModel { Ukprn = ukprn });

        result.ShouldNotHaveAnyValidationErrors();
        ukprnServiceMock.Verify(s => s.GetOrganisationAsync(ukprn, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenValidatingUkprn_AndServiceReturnsNull_ThenHasInvalidUkprnError(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] UkprnValidator sut)
    {
        const int ukprn = 10007938;
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);

        var result = await sut.TestValidateAsync(new UkprnModel { Ukprn = ukprn });

        result.ShouldHaveValidationErrorFor(x => x.Ukprn)
            .WithErrorMessage(UkprnValidator.UkprnInvalidValidationMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenValidatingUkprn_AndUkprnIsZero_ThenHasEmptyErrorWithoutCallingService(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] UkprnValidator sut)
    {
        var result = await sut.TestValidateAsync(new UkprnModel { Ukprn = 0 });

        result.ShouldHaveValidationErrorFor(x => x.Ukprn)
            .WithErrorMessage(UkprnValidator.UkprnEmptyValidationMessage);
        ukprnServiceMock.Verify(s => s.GetOrganisationAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    [MoqInlineAutoData(1234567)]
    [MoqInlineAutoData(23456789)]
    [MoqInlineAutoData(123456789)]
    public async Task WhenValidatingUkprn_AndUkprnFormatIsInvalid_ThenHasInvalidErrorWithoutCallingService(
        int ukprn,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Greedy] UkprnValidator sut)
    {
        var result = await sut.TestValidateAsync(new UkprnModel { Ukprn = ukprn });

        result.ShouldHaveValidationErrorFor(x => x.Ukprn)
            .WithErrorMessage(UkprnValidator.UkprnInvalidValidationMessage);
        ukprnServiceMock.Verify(s => s.GetOrganisationAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
