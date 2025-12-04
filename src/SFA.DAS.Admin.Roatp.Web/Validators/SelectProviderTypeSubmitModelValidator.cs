using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class SelectProviderTypeSubmitModelValidator : AbstractValidator<SelectProviderTypeSubmitModel>
{
    public const string NoProviderTypeSelectedErrorMessage = "Select a route for this provider";

    public SelectProviderTypeSubmitModelValidator()
    {
        RuleFor(s => s.SelectedProviderTypeId)
            .NotEmpty()
            .WithMessage(NoProviderTypeSelectedErrorMessage);
    }
}
