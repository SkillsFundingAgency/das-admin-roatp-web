using System.ComponentModel;

namespace SFA.DAS.Admin.Roatp.Domain.Models;

public enum LearningType
{
    [Description("Apprenticeship")]
    Apprenticeship = 1,

    [Description("Apprenticeship unit")]
    ApprenticeshipUnit = 2,

    [Description("Foundation apprenticeship")]
    FoundationApprenticeship = 3
}
