using System.Text.RegularExpressions;
using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public partial class UkprnValidator : AbstractValidator<IUkprn>
{
    public const string UkprnEmptyValidationMessage = "UKPRN is empty";
    public const string UkprnInvalidValidationMessage = "UKPRN is invalid";

    [GeneratedRegex(@"^1\d{7}$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex UkprnFormatRegex();

    public UkprnValidator(IUkprnService ukprnService)
    {
        RuleFor(model => model.Ukprn)
            .Cascade(CascadeMode.Stop)
            .Must(ukprn => ukprn != 0)
            .WithMessage(UkprnEmptyValidationMessage)
            .Must(ukprn => UkprnFormatRegex().IsMatch(ukprn.ToString()))
            .WithMessage(UkprnInvalidValidationMessage)
            .MustAsync(async (ukprn, cancellationToken) =>
                await ukprnService.GetOrganisationAsync(ukprn, cancellationToken) is not null)
            .WithMessage(UkprnInvalidValidationMessage);
    }
}
