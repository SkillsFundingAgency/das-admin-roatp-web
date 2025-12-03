using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class OfferApprenticeshipUnitsSubmitModelValidator : AbstractValidator<OfferApprenticeshipUnitsSubmitModel>
{
    public const string ApprenticeshipUnitSelectionErrorMessage = "Select if this provider offers apprenticeship units";

    public OfferApprenticeshipUnitsSubmitModelValidator()
    {
        RuleFor(s => s.IsApprenticeshipUnitsOffered)
            .NotNull()
            .WithMessage(ApprenticeshipUnitSelectionErrorMessage);
    }
}