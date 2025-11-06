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
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(NoValueSelectedErrorMessage)
            .Equal(true)
            .When(s => !s.OffersApprenticeships)
            .WithMessage(NoSelectionsMadeErrorMessage);
    }
}