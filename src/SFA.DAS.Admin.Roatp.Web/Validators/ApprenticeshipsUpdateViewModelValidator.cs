using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ApprenticeshipsUpdateViewModelValidator : AbstractValidator<ApprenticeshipsUpdateViewModel>
{
    public const string ApprenticeshipsSelectionErrorMessage = "Select if this provider offers apprenticeships";

    public ApprenticeshipsUpdateViewModelValidator()
    {
        RuleFor(s => s.ApprenticeshipsSelectionChoice)
            .NotEmpty()
            .WithMessage(ApprenticeshipsSelectionErrorMessage);
    }
}