using System.Security.Claims;
using FluentValidation;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class RestrictedCourseProviderService(
    IValidator<IUkprnAndLarsCodeValidator> ukprnAndLarsCodeValidator,
    ILarsCodeService larsCodeService) : IRestrictedCourseProviderService
{
    public async Task<bool> IsRouteValidAsync(
        string larsCode,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = await ukprnAndLarsCodeValidator.ValidateAsync(
            new UkprnAndLarsCodeModel { Ukprn = ukprn, LarsCode = larsCode },
            cancellationToken);

        return validationResult.IsValid;
    }

    public async Task<(GetRestrictedCourseDetailsResponse Course, ProviderCourseModel Provider)?> GetCourseAndProviderAsync(
        string larsCode,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var courseDetails = await larsCodeService.GetCourseDetailsAsync(larsCode, cancellationToken);
        var provider = courseDetails?.Providers.FirstOrDefault(p => p.Ukprn == ukprn);

        if (courseDetails is null || provider is null)
        {
            return null;
        }

        return (courseDetails, provider);
    }

    public UpsertProviderAllowedCourseRequest CreateUpsertRequest(
        ClaimsPrincipal user,
        DateTime? lastDateStarts)
        => new()
        {
            UserId = user.UserId(),
            UserDisplayName = user.UserDisplayName(),
            LastDateStarts = lastDateStarts
        };
}
