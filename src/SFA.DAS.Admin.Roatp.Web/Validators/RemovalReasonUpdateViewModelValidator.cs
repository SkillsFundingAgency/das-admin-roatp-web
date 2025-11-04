using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class RemovalReasonUpdateViewModelValidator : AbstractValidator<RemovalReasonUpdateViewModel>
{
    public const string NoRemovalReasonSelectedErrorMessage = "Select a reason for removal";

    public RemovalReasonUpdateViewModelValidator()
    {
        RuleFor(s => s.RemovalReasonId)
            .NotEmpty()
            .WithMessage(NoRemovalReasonSelectedErrorMessage);
    }
}