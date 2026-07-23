using System.ComponentModel;
using System.Reflection;
using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class EnumExtensions
{

    public static string GetDescription(this Enum enumValue)
    {
        return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
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
}
