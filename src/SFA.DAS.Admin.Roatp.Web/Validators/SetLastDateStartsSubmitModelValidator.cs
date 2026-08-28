using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class SetLastDateStartsSubmitModelValidator : AbstractValidator<SetLastDateStartsSubmitModel>
{
    public static readonly DateTime MinimumLastDateStarts = new(2014, 9, 1, 0, 0, 0, DateTimeKind.Unspecified);

    public const string EnterValidDateErrorMessage = "Enter a valid date";
    public const string DateMustBeAfterMinimumErrorMessage = "The last start date must be on or after 1 September 2014";
    public const string DateFieldName = nameof(SetLastDateStartsSubmitModel.Day);

    public SetLastDateStartsSubmitModelValidator()
    {
        RuleFor(model => model)
            .Custom((model, context) =>
            {
                if (!model.TryGetEnteredDate(out var enteredDate))
                {
                    context.AddFailure(DateFieldName, EnterValidDateErrorMessage);
                    return;
                }

                if (enteredDate.Date < MinimumLastDateStarts)
                {
                    context.AddFailure(DateFieldName, DateMustBeAfterMinimumErrorMessage);
                    return;
                }

                if (model.CourseLastDateStarts.HasValue
                    && enteredDate.Date > model.CourseLastDateStarts.Value.Date)
                {
                    context.AddFailure(
                        DateFieldName,
                        $"The latest start date for this course is {model.CourseLastDateStarts.Value.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
                }
            });
    }
}
