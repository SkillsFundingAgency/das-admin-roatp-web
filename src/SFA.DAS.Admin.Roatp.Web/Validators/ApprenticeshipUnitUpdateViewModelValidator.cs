using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ApprenticeshipUnitUpdateViewModelValidator : AbstractValidator<ApprenticeshipUnitsUpdateViewModel>
{
    public const string NoSelectionsMadeErrorMessage = "Training providers must offer either apprenticeships or apprenticeship units";

    public ApprenticeshipUnitUpdateViewModelValidator()
    {
        RuleFor(s => s.ApprenticeshipUnitsSelectionId)
            .Equal(true)
            .When(s => !s.OffersApprenticeships)
            .WithMessage(NoSelectionsMadeErrorMessage);
    }
}