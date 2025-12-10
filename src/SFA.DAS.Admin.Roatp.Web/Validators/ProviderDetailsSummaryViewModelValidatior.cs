using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ProviderDetailsSummaryViewModelValidatior : AbstractValidator<ProviderDetailsSummaryViewModel>
{
    public const string RequiredCourseTypeSelectionErrorMessage = "Training providers must offer either apprenticeships or apprenticeship units to join the register";
    public const string RequiredFieldsNotSetErrorMessage = "You need to complete the rest of the questions before you can add this training provider";

    public ProviderDetailsSummaryViewModelValidatior()
    {
        RuleFor(s => s)
            .Must(s => !(s.IsSupportingProvider == false &&
            s.OffersApprenticeshipsText == "No" &&
            s.OffersApprenticeshipUnitsText == "No"))
            .WithMessage(RequiredCourseTypeSelectionErrorMessage);

        RuleFor(s => s)
            .Must(s => !(s.IsSupportingProvider == false &&
            (s.OffersApprenticeshipsText == null ||
            s.OffersApprenticeshipUnitsText == null)))
            .WithMessage(RequiredFieldsNotSetErrorMessage);

        RuleFor(s => s.ProviderRoute)
            .NotEmpty()
            .WithMessage(RequiredFieldsNotSetErrorMessage);

        RuleFor(s => s.OrganisationType)
            .NotEmpty()
            .WithMessage(RequiredFieldsNotSetErrorMessage);
    }
}
