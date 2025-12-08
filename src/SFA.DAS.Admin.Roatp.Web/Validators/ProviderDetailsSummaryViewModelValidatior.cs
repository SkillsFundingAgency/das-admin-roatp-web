using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ProviderDetailsSummaryViewModelValidatior : AbstractValidator<ProviderDetailsSummaryViewModel>
{
    public const string RequiredCourseTypeSelectionErrorMessage = "Training providers must offer either apprenticeships or apprenticeship units to join the register";

    public ProviderDetailsSummaryViewModelValidatior()
    {
        RuleFor(s => s.IsSupportingProvider)
            .Equal(true)
            .When(s => s.OffersApprenticeshipsText == "No" && s.OffersApprenticeshipUnitsText == "No")
            .WithMessage(RequiredCourseTypeSelectionErrorMessage);
    }
}
