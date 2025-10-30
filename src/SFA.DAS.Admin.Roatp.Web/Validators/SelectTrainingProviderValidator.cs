using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class SelectTrainingProviderValidator : AbstractValidator<SelectTrainingProviderViewModel>
{
    public const string NoTrainingProviderSelectedErrorMessage = "Type a name or UKPRN and select a provider";

    public SelectTrainingProviderValidator()
    {
        RuleFor(s => s.SearchTerm)
            .NotEmpty()
            .WithMessage(NoTrainingProviderSelectedErrorMessage);
    }
}