using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class RestrictedCourseChangeRestrictionSubmitModelValidator : AbstractValidator<RestrictedCourseChangeRestrictionSubmitModel>
{
    public const string NoOptionSelectedErrorMessage = "You must select an option";

    public RestrictedCourseChangeRestrictionSubmitModelValidator()
    {
        RuleFor(model => model.SelectedOption)
            .Must(option => option == RestrictedCourseChangeRestrictionOptions.AddOrChange
                || option == RestrictedCourseChangeRestrictionOptions.Remove)
            .WithMessage(NoOptionSelectedErrorMessage);
    }
}
