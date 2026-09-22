using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class RestrictCourseSearchSubmitModelValidator : AbstractValidator<RestrictCourseSearchSubmitModel>
{
    public const string NoCourseSelectedErrorMessage = "Enter a course name in the search box and select one from the list";

    public RestrictCourseSearchSubmitModelValidator()
    {
        RuleFor(model => model.SelectedLarsCode)
            .NotEmpty()
            .WithMessage(NoCourseSelectedErrorMessage);
    }
}
