namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class DateTimeExtensions
{
    public static string ToDisplayString(this DateTime date) => date.ToString("dd MMM yyyy");
}
