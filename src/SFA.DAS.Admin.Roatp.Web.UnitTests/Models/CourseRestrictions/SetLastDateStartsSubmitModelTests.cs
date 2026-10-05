using AutoFixture.NUnit4;
using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.CourseRestrictions;

[TestFixture]
public class SetLastDateStartsSubmitModelTests
{
    [Test]
    [InlineAutoData("abc", "06", "2027")]
    [InlineAutoData("15", "abc", "2027")]
    [InlineAutoData("15", "06", "abc")]
    [InlineAutoData("31", "02", "2027")]
    [InlineAutoData("", "", "")]
    [InlineAutoData(null, null, null)]
    public void WhenEnteredDateIsInvalid_ThenTryGetEnteredDateReturnsFalse(string? day, string? month, string? year)
    {
        var model = new SetLastDateStartsSubmitModel { Day = day, Month = month, Year = year };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeFalse();
        date.Should().Be(default(DateTime));
    }

    [Test]
    [InlineAutoData("15", "06", "2027")]
    public void WhenEnteredDateIsValid_ThenTryGetEnteredDateReturnsTrue(string day, string month, string year)
    {
        var model = new SetLastDateStartsSubmitModel { Day = day, Month = month, Year = year };

        var result = model.TryGetEnteredDate(out var date);

        result.Should().BeTrue();
        date.Should().Be(new DateTime(2027, 6, 15, 0, 0, 0, DateTimeKind.Unspecified));
    }

    [Test]
    [InlineAutoData("15", "06", "2027")]
    public void WhenGettingEnteredDate_AndDateIsValid_ThenReturnsParsedDate(string day, string month, string year)
    {
        var model = new SetLastDateStartsSubmitModel { Day = day, Month = month, Year = year };

        var date = model.GetEnteredDate();

        date.Should().Be(new DateTime(2027, 6, 15, 0, 0, 0, DateTimeKind.Unspecified));
    }

    [Test]
    [InlineAutoData("abc", "06", "2027")]
    [InlineAutoData("15", "abc", "2027")]
    [InlineAutoData("15", "06", "abc")]
    [InlineAutoData("31", "02", "2027")]
    [InlineAutoData("", "", "")]
    [InlineAutoData(null, null, null)]
    public void WhenGettingEnteredDate_AndDateIsInvalid_ThenThrows(string? day, string? month, string? year)
    {
        var model = new SetLastDateStartsSubmitModel { Day = day, Month = month, Year = year };

        var act = () => model.GetEnteredDate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Last start date must be valid before it is read.");
    }
}
