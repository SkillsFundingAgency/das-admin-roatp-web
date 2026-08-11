using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class UnrestrictedCourseProviderViewModel
{
    public int Ukprn { get; set; }
    public required string ProviderName { get; set; }

    public static implicit operator UnrestrictedCourseProviderViewModel(ProviderCourseModel provider) => new()
    {
        Ukprn = provider.Ukprn,
        ProviderName = provider.ProviderName
    };
}
