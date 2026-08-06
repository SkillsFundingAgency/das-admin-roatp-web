using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class UnrestrictedCourseSearchValidator : AbstractValidator<UnrestrictedCourseSearchViewModel>
{
    public const string NoCourseSelectedErrorMessage =
        "Enter a course name in the search box and select one from the list";

    public UnrestrictedCourseSearchValidator()
    {
        RuleFor(s => s.SearchTerm)
            .NotEmpty()
            .WithMessage(NoCourseSelectedErrorMessage);
    }
}
