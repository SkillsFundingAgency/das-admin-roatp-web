namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class DateTimeExtensions
{
    public static string ToScreenString(this DateTime date) => date.ToString("dd MMM yyyy");
}
