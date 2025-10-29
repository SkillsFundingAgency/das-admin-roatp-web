using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;
public class DateTimeExtensionsTests
{

    [TestCaseSource(nameof(_testCases))]
    public void ToScreenString_Expected(DateTime dateToCheck, string expectedResult)
    {
        var result = dateToCheck.ToScreenString();
        result.Should().Be(expectedResult);
    }

    private static object[] _testCases =
    {
        new object[] { new DateTime(2024,1,1), "01 Jan 2024" },
        new object[] { new DateTime(2024,12,31), "31 Dec 2024" }
    };
}
