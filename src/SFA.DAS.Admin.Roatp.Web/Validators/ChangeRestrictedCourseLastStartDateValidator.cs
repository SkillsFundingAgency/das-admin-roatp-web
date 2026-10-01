using System.Net;
using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class ChangeRestrictedCourseLastStartDateValidator
    : AbstractValidator<ChangeRestrictedCourseLastStartDateModel>
{
    public const string ProviderMustBeUnrestrictedErrorMessage = "The provider must be unrestricted";
    public const string CourseMustBeRestrictedForProviderErrorMessage =
        "The course must be restricted for the provider";

    public ChangeRestrictedCourseLastStartDateValidator(IOuterApiClient outerApiClient)
    {
        RuleFor(model => model)
            .CustomAsync(async (model, context, cancellationToken) =>
            {
                var response = await outerApiClient.GetRestrictedApprenticeships(model.Ukprn, cancellationToken);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    context.AddFailure(
                        nameof(ChangeRestrictedCourseLastStartDateModel.Ukprn),
                        ProviderMustBeUnrestrictedErrorMessage);
                    return;
                }

                var courses = response.Content?.Courses ?? [];
                var isCourseRestrictedForProvider = courses.Any(course => course.LarsCode == model.LarsCode);
                if (!isCourseRestrictedForProvider)
                {
                    context.AddFailure(CourseMustBeRestrictedForProviderErrorMessage);
                }
            });
    }
}
