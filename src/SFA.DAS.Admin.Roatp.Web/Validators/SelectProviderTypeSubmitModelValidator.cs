using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class SelectProviderTypeSubmitModelValidator : AbstractValidator<SelectProviderTypeSubmitModel>
{
    public const string NoProviderTypeSelectedErrorMessage = "Select a route for this provide";

    public SelectProviderTypeSubmitModelValidator()
    {
        RuleFor(s => s.ProviderTypeId)
            .NotEmpty()
            .WithMessage(NoProviderTypeSelectedErrorMessage);
    }
}
