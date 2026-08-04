using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class AddLastDateStartsViewModelValidator : AbstractValidator<AddLastDateStartsViewModel>
{
    public static readonly DateTime MinimumLastDateStarts = new(2014, 9, 1);

    public const string EnterValidDateErrorMessage = "Enter a valid date";
    public const string DateMustBeAfterMinimumErrorMessage = "The last start date must be after 1 September 2014";
    public const string DateFieldName = nameof(AddLastDateStartsViewModel.Day);

    public AddLastDateStartsViewModelValidator()
    {
        RuleFor(model => model)
            .Custom((model, context) =>
            {
                if (!model.TryGetEnteredDate(out var enteredDate))
                {
                    context.AddFailure(DateFieldName, EnterValidDateErrorMessage);
                    return;
                }

                if (enteredDate.Date <= MinimumLastDateStarts)
                {
                    context.AddFailure(DateFieldName, DateMustBeAfterMinimumErrorMessage);
                    return;
                }

                if (model.CourseLastDateStarts.HasValue
                    && enteredDate.Date >= model.CourseLastDateStarts.Value.Date)
                {
                    context.AddFailure(
                        DateFieldName,
                        $"The latest start date for this course is {model.CourseLastDateStarts.Value.ToScreenString()}. It is set by LARs and cannot be changed. Your chosen last date for new starts must come before this.");
                }
            });
    }
}
