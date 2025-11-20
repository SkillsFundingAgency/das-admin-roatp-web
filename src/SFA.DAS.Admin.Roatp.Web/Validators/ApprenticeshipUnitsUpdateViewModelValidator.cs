using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ApprenticeshipUnitsUpdateViewModelValidator : AbstractValidator<ApprenticeshipUnitsUpdateViewModel>
{
    public const string NoSelectionsMadeErrorMessage = "Training providers must offer either apprenticeships or apprenticeship units";
    public const string NoValueSelectedErrorMessage = "Select if this provider offers apprenticeship units";

    public ApprenticeshipUnitsUpdateViewModelValidator()
    {
        RuleFor(s => s.ApprenticeshipUnitsSelectionId)
            .NotNull()
            .WithMessage(NoValueSelectedErrorMessage);


        RuleFor(s => s.ApprenticeshipUnitsSelectionId)
            .Equal(true)
            .When(s => !s.OffersApprenticeships && s.ApprenticeshipUnitsSelectionId != null)
            .WithMessage(NoSelectionsMadeErrorMessage);
    }
}