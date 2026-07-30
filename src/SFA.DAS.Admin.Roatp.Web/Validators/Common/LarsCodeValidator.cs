using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public class LarsCodeValidator : AbstractValidator<ILarsCode>
{
    public const string LarsCodeEmptyValidationMessage = "LARS code is empty";
    public const string LarsCodeInvalidValidationMessage = "LARS code is invalid";

    public LarsCodeValidator(ILarsCodeService larsCodeService)
    {
        RuleFor(model => model.LarsCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(LarsCodeEmptyValidationMessage)
            .MustAsync(async (larsCode, cancellationToken) =>
                await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken) is not null)
            .WithMessage(LarsCodeInvalidValidationMessage);
    }
}
