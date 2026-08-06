using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class SearchCourseToRestrictValidator : AbstractValidator<SearchCourseToRestrictViewModel>
{
    public const string NoCourseSelectedErrorMessage =
        "Enter a course name in the search box and select one from the list";

    public SearchCourseToRestrictValidator()
    {
        RuleFor(s => s.SearchTerm)
            .NotEmpty()
            .WithMessage(NoCourseSelectedErrorMessage);
    }
}
