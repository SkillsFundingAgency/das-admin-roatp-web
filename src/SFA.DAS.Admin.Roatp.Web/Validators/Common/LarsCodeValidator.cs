using FluentValidation;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public class LarsCodeValidator : AbstractValidator<string>
{
    public const string LarsCodeValidationMessage = "Invalid LARS code";

    private readonly IOuterApiClient _outerApiClient;

    public LarsCodeValidator(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;

        RuleFor(larsCode => larsCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(LarsCodeValidationMessage)
            .MustAsync(BeValidLarsCode)
            .WithMessage(LarsCodeValidationMessage);
    }

    public async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(larsCode))
        {
            return null;
        }

        var response = await _outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            return null;
        }

        return response.Content;
    }

    private async Task<bool> BeValidLarsCode(string larsCode, CancellationToken cancellationToken)
        => await GetCourseDetailsAsync(larsCode, cancellationToken) is not null;
}
