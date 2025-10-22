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
        new object[] { new DateTime(2025,10,10), "10 Oct 2025" },
        new object[] { new DateTime(2024,1,1), "01 Jan 2024" },
        new object[] { new DateTime(2024,2,3), "03 Feb 2024" },
        new object[] { new DateTime(2024,3,9), "09 Mar 2024" },
        new object[] { new DateTime(2024,4,10), "10 Apr 2024" },
        new object[] { new DateTime(2024,5,19), "19 May 2024" },
        new object[] { new DateTime(2024,6,20), "20 Jun 2024" },
        new object[] { new DateTime(2024,7,21), "21 Jul 2024" },
        new object[] { new DateTime(2024,8,30), "30 Aug 2024" },
        new object[] { new DateTime(2024,9,1), "01 Sep 2024" },
        new object[] { new DateTime(2024,10,31), "31 Oct 2024" },
        new object[] { new DateTime(2024,11,1), "01 Nov 2024" },
        new object[] { new DateTime(2024,12,31), "31 Dec 2024" }
    };
}
