using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ChangeCourseRestrictionSubmitModelValidator : AbstractValidator<ChangeCourseRestrictionSubmitModel>
{
    public const string NoOptionSelectedErrorMessage = "You must select an option";

    public ChangeCourseRestrictionSubmitModelValidator()
    {
        RuleFor(model => model.SelectedOption)
            .Must(option =>
                option == ChangeCourseRestrictionOptions.Change
                || option == ChangeCourseRestrictionOptions.Remove)
            .WithMessage(NoOptionSelectedErrorMessage);
    }
}
