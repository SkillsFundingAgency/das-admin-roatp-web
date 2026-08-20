using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ChangeLastDateStartsSubmitModelValidator : AbstractValidator<ChangeLastDateStartsSubmitModel>
{
    public const string NoOptionSelectedErrorMessage = "You must select an option";

    public ChangeLastDateStartsSubmitModelValidator()
    {
        RuleFor(model => model.SelectedOption)
            .Must(option =>
                option == ChangeLastDateStartsOptions.Change
                || option == ChangeLastDateStartsOptions.Remove)
            .WithMessage(NoOptionSelectedErrorMessage);
    }
}
