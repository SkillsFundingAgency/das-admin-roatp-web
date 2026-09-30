using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ProviderRestrictedCourseSearchSubmitModelValidator : AbstractValidator<ProviderRestrictedCourseSearchSubmitModel>
{
    public const string NoCourseSelectedErrorMessage = "Enter a course name in the search box and select one from the list";

    public ProviderRestrictedCourseSearchSubmitModelValidator()
    {
        RuleFor(model => model.SelectedLarsCode)
            .NotEmpty()
            .WithMessage(NoCourseSelectedErrorMessage);
    }
}
