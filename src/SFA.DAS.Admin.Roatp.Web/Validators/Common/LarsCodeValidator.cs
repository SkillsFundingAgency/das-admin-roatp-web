using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public class LarsCodeValidator : AbstractValidator<string>
{
    public const string LarsCodeValidationMessage = "Invalid LARS code";

    public LarsCodeValidator(ILarsCodeService larsCodeService)
    {
        RuleFor(larsCode => larsCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(LarsCodeValidationMessage)
            .MustAsync(async (larsCode, cancellationToken) =>
                await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken) is not null)
            .WithMessage(LarsCodeValidationMessage);
    }
}
