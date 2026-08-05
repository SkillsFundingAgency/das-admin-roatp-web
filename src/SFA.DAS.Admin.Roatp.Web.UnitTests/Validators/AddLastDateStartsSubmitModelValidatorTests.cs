using FluentAssertions;
using FluentValidation.TestHelper;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Validators;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Validators;

[TestFixture]
public class AddLastDateStartsSubmitModelValidatorTests
{
    private AddLastDateStartsSubmitModelValidator _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new AddLastDateStartsSubmitModelValidator();

    [Test]
    public void WhenDateFieldsAreBlank_ThenReturnsEnterValidDateError()
    {
        var model = CreateModel();

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
    }

    [Test]
    public void WhenDateFieldsArePartiallyComplete_ThenReturnsEnterValidDateError()
    {
        var model = CreateModel(day: "15", month: "06", year: "");

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
    }

    [Test]
    public void WhenDateIsInvalid_ThenReturnsEnterValidDateError()
    {
        var model = CreateModel(day: "31", month: "02", year: "2027");

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
    }

    [Test]
    public void WhenDateIsOnOrBeforeMinimum_ThenReturnsMinimumDateError()
    {
        var model = CreateModel(day: "01", month: "09", year: "2014");

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.DateMustBeAfterMinimumErrorMessage);
    }

    [Test]
    public void WhenDateIsBeforeMinimum_ThenReturnsMinimumDateError()
    {
        var model = CreateModel(day: "31", month: "08", year: "2014");

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(AddLastDateStartsSubmitModelValidator.DateMustBeAfterMinimumErrorMessage);
    }

    [Test]
    public void WhenDateIsOnOrAfterCourseLastDateStarts_ThenReturnsLarsEndDateError()
    {
        var courseEndDate = new DateTime(2027, 6, 1);
        var model = CreateModel(day: "01", month: "06", year: "2027", courseLastDateStarts: courseEndDate);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(AddLastDateStartsSubmitModelValidator.DateFieldName)
            .WithErrorMessage(
                $"The latest start date for this course is {courseEndDate.ToScreenString()}. It is set by LARs and cannot be changed. Your chosen last date for new starts must come before this.");
    }

    [Test]
    public void WhenDateIsValidAndBeforeCourseEndDate_ThenPasses()
    {
        var model = CreateModel(
            day: "15",
            month: "03",
            year: "2027",
            courseLastDateStarts: new DateTime(2027, 6, 1));

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void WhenDateIsValidAndCourseHasNoEndDate_ThenPasses()
    {
        var model = CreateModel(day: "15", month: "03", year: "2027");

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static AddLastDateStartsSubmitModel CreateModel(
        string day = "",
        string month = "",
        string year = "",
        DateTime? courseLastDateStarts = null)
        => new()
        {
            Day = day,
            Month = month,
            Year = year,
            CourseLastDateStarts = courseLastDateStarts
        };
}
