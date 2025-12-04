using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class OrganisationTypeSubmitModelValidator : AbstractValidator<OrganisationTypeSubmitModel>
{
    public const string OrganisationTypeSelectionErrorMessage = "Choose organisation type";

    public OrganisationTypeSubmitModelValidator()
    {
        RuleFor(s => s.OrganisationTypeId)
            .NotNull()
            .WithMessage(OrganisationTypeSelectionErrorMessage);
    }
}
