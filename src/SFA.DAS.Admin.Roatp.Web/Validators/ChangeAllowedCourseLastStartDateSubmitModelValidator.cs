using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ChangeAllowedCourseLastStartDateSubmitModelValidator : AbstractValidator<ChangeAllowedCourseLastStartDateSubmitModel>
{
    public const string NoOptionSelectedErrorMessage = "You must select an option";

    public ChangeAllowedCourseLastStartDateSubmitModelValidator()
    {
        RuleFor(model => model.SelectedOption)
            .Must(option =>
                option == ChangeAllowedCourseLastStartDateOptions.Change
                || option == ChangeAllowedCourseLastStartDateOptions.Remove)
            .WithMessage(NoOptionSelectedErrorMessage);
    }
}
