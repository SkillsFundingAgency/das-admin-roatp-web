using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ChangeProviderRestrictedCourseSubmitModelValidator : AbstractValidator<ChangeProviderRestrictedCourseSubmitModel>
{
    public const string NoOptionSelectedErrorMessage = "You must select an option";

    public ChangeProviderRestrictedCourseSubmitModelValidator()
    {
        RuleFor(model => model.SelectedOption)
            .Must(option => option == ChangeProviderRestrictedCourseOptions.AddOrChange
                || option == ChangeProviderRestrictedCourseOptions.Remove)
            .WithMessage(NoOptionSelectedErrorMessage);
    }
}
