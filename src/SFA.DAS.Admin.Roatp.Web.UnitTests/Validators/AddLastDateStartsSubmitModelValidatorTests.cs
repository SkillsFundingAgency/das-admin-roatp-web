using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class AddLastDateStartsSubmitModelValidatorTests
{
    private Mock<ILarsCodeService> _larsCodeServiceMock = null!;
    private AddLastDateStartsSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _larsCodeServiceMock = new Mock<ILarsCodeService>();
        _sut = new AddLastDateStartsSubmitModelValidator(_larsCodeServiceMock.Object);
    }

    [Test]
    public async Task WhenDateFieldsAreBlank_ThenReturnsEnterValidDateError()
    {
        var model = CreateModel();

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
        _larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task WhenDateFieldsArePartiallyComplete_ThenReturnsEnterValidDateError()
    {
        var model = CreateModel(day: "15", month: "06", year: "");

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
        _larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task WhenDateIsInvalid_ThenReturnsEnterValidDateError()
    {
        var model = CreateModel(day: "31", month: "02", year: "2027");

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
        _larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task WhenDateEqualsMinimum_ThenPasses()
    {
        var model = CreateModel(day: "01", month: "09", year: "2014");

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName);
    }

    [Test]
    public async Task WhenDateIsBeforeMinimum_ThenReturnsMinimumDateError()
    {
        var model = CreateModel(day: "31", month: "08", year: "2014");

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.DateMustBeAfterMinimumErrorMessage);
        _larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task WhenDateIsAfterCourseLastDateStarts_ThenReturnsLarsEndDateError()
    {
        var courseEndDate = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var model = CreateModel(day: "02", month: "06", year: "2027", courseLastDateStarts: courseEndDate);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(
                $"The latest start date for this course is {courseEndDate.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
        _larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task WhenDateIsAfterCourseLastDateStartsFromApi_ThenReturnsLarsEndDateError()
    {
        const string larsCode = "123";
        var courseEndDate = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
        _larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCourseDetailsResponse
            {
                LarsCode = larsCode,
                IfateReferenceNumber = "ST0001",
                CourseName = "Test course",
                Route = "Test route",
                LastDateStarts = courseEndDate
            });

        var model = CreateModel(day: "02", month: "06", year: "2027", larsCode: larsCode);

        var result = await _sut.TestValidateAsync(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(
                $"The latest start date for this course is {courseEndDate.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
        _larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(larsCode, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task WhenDateIsValidAndBeforeCourseEndDate_ThenPasses()
    {
        var model = CreateModel(
            day: "15",
            month: "03",
            year: "2027",
            courseLastDateStarts: new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified));

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task WhenDateIsValidAndCourseHasNoEndDate_ThenPasses()
    {
        var model = CreateModel(day: "15", month: "03", year: "2027");

        var result = await _sut.TestValidateAsync(model);

        result.ShouldNotHaveAnyValidationErrors();
        _larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AddLastDateStartsSubmitModel CreateModel(
        string day = "",
        string month = "",
        string year = "",
        string? larsCode = null,
        DateTime? courseLastDateStarts = null)
        => new()
        {
            Day = day,
            Month = month,
            Year = year,
            LarsCode = larsCode,
            CourseLastDateStarts = courseLastDateStarts
        };
}
