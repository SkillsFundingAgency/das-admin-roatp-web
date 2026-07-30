using System.ComponentModel;
using System.Reflection;
using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        return enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? string.Empty;
    }

    public static string GetTagClass(this LearningType learningType)
    {
        return learningType switch
        {
            LearningType.Apprenticeship => "govuk-tag--blue",
            LearningType.FoundationApprenticeship => "govuk-tag--pink",
            LearningType.ApprenticeshipUnit => "govuk-tag--purple",
            _ => string.Empty
        };
    }

    public static string GetTagClass(this DeliveryStatus deliveryStatus)
    {
        return deliveryStatus switch
        {
            DeliveryStatus.OpenToNewStarts => "govuk-tag--green",
            DeliveryStatus.LastStartDateAdded => "govuk-tag--orange",
            DeliveryStatus.ClosedToNewStarts => "govuk-tag--grey",
            _ => string.Empty
        };
    }

    public static DeliveryStatus ToDeliveryStatus(this DateTime? lastDateStarts, DateTime? today = null)
    {
        if (!lastDateStarts.HasValue)
        {
            return DeliveryStatus.OpenToNewStarts;
        }

        var comparisonDate = (today ?? DateTime.UtcNow).Date;
        return lastDateStarts.Value.Date >= comparisonDate
            ? DeliveryStatus.LastStartDateAdded
            : DeliveryStatus.ClosedToNewStarts;
    }
}
