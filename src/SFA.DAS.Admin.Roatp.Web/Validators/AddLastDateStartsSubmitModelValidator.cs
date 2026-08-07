using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class AddLastDateStartsSubmitModelValidator : AbstractValidator<AddLastDateStartsSubmitModel>
{
    public static readonly DateTime MinimumLastDateStarts = new(2014, 9, 1, 0, 0, 0, DateTimeKind.Unspecified);

    public const string EnterValidDateErrorMessage = "Enter a valid date";
    public const string DateMustBeAfterMinimumErrorMessage = "The last start date must be on or after 1 September 2014";
    public const string DateFieldName = nameof(AddLastDateStartsSubmitModel.Day);

    public AddLastDateStartsSubmitModelValidator(ILarsCodeService larsCodeService)
    {
        RuleFor(model => model)
            .CustomAsync(async (model, context, cancellationToken) =>
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

                var courseLastDateStarts = model.CourseLastDateStarts;
                if (!courseLastDateStarts.HasValue && !string.IsNullOrWhiteSpace(model.LarsCode))
                {
                    var courseDetails = await larsCodeService.GetCourseDetailsAsync(model.LarsCode, cancellationToken);
                    courseLastDateStarts = courseDetails?.LastDateStarts;
                }

                if (courseLastDateStarts.HasValue
                    && enteredDate.Date > courseLastDateStarts.Value.Date)
                {
                    context.AddFailure(
                        DateFieldName,
                        $"This course has an operational end date in LARs. It has been set by Skills England for {courseLastDateStarts.Value.ToScreenString()}. Your last date for new starts must come on or before this date.");
                }
            });
    }
}
