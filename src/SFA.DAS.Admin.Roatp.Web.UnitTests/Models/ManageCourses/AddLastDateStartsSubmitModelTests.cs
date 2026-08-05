using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class AddLastDateStartsSubmitModelTests
{
    [Test]
    public void WhenDayIsNotValidInteger_AndMonthAndYearAreValid_ThenTryGetEnteredDateReturnsFalse()
    {
        var model = new AddLastDateStartsSubmitModel { Day = "abc", Month = "06", Year = "2027" };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeFalse();
        date.Should().Be(default(DateTime));
    }

    [Test]
    public void WhenMonthIsNotValidInteger_AndDayAndYearAreValid_ThenTryGetEnteredDateReturnsFalse()
    {
        var model = new AddLastDateStartsSubmitModel { Day = "15", Month = "abc", Year = "2027" };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeFalse();
        date.Should().Be(default(DateTime));
    }

    [Test]
    public void WhenYearIsNotValidInteger_AndDayAndMonthAreValid_ThenTryGetEnteredDateReturnsFalse()
    {
        var model = new AddLastDateStartsSubmitModel { Day = "15", Month = "06", Year = "abc" };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeFalse();
        date.Should().Be(default(DateTime));
    }

    [Test]
    public void WhenDayMonthAndYearAreValidIntegers_ThenTryGetEnteredDateReturnsTrue()
    {
        var model = new AddLastDateStartsSubmitModel { Day = "15", Month = "06", Year = "2027" };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeTrue();
        date.Should().Be(new DateTime(2027, 6, 15, 0, 0, 0, DateTimeKind.Unspecified));
    }

    [Test]
    public void WhenDayMonthAndYearAreValidIntegers_DateIsOutOfRange_ThenTryGetEnteredDateReturnsFalse()
    {
        var model = new AddLastDateStartsSubmitModel { Day = "31", Month = "02", Year = "2027" };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeFalse();
        date.Should().Be(default(DateTime));
    }

    [Test]
    public void WhenDayMonthAndYearAreEmpty_ThenTryGetEnteredDateReturnsFalse()
    {
        var model = new AddLastDateStartsSubmitModel { Day = "", Month = "", Year = "" };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeFalse();
        date.Should().Be(default(DateTime));
    }
    [Test]
    public void WhenDayMonthAndYearAreNull_ThenTryGetEnteredDateReturnsFalse()
    {
        var model = new AddLastDateStartsSubmitModel { Day = null, Month = null, Year = null };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeFalse();
        date.Should().Be(default(DateTime));
    }
}
