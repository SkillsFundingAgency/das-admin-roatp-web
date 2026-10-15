using System.ComponentModel;

namespace SFA.DAS.Admin.Roatp.Domain.Models;

public enum RouteLearningType
{
    [Description("Apprenticeship")]
    Apprenticeship = 1,

    [Description("Apprenticeship unit")]
    ApprenticeshipUnit = 2
}
