using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class OfferApprenticeshipsSubmitModelValidator : AbstractValidator<OfferApprenticeshipsSubmitModel>
{
    public const string ApprenticeshipsSelectionErrorMessage = "Select if this provider offers apprenticeships";

    public OfferApprenticeshipsSubmitModelValidator()
    {
        RuleFor(s => s.ApprenticeshipsSelectionChoice)
            .NotEmpty()
            .WithMessage(ApprenticeshipsSelectionErrorMessage);
    }
}