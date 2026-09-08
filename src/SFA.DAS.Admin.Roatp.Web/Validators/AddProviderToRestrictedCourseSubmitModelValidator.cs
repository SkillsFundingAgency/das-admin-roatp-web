using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class AddProviderToRestrictedCourseSubmitModelValidator : AbstractValidator<AddProviderToRestrictedCourseSubmitModel>
{
    public const string NoProviderSelectedErrorMessage =
        "Enter a provider's name or UKPRN and select it from the list";

    public AddProviderToRestrictedCourseSubmitModelValidator()
    {
        RuleFor(model => model.SelectedUkprn)
            .NotEmpty()
            .WithMessage(NoProviderSelectedErrorMessage);
    }
}
