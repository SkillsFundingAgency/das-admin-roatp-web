using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class AddAllowedCourseSubmitModelValidator : AbstractValidator<AddAllowedCourseSubmitModel>
{
    public const string NoCourseSelectedErrorMessage = "Enter a course name in the search box and select one from the list";

    public AddAllowedCourseSubmitModelValidator()
    {
        RuleFor(model => model.SelectedLarsCode)
            .NotEmpty()
            .WithMessage(NoCourseSelectedErrorMessage);
    }
}
