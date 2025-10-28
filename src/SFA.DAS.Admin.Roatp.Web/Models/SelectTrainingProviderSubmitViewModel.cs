namespace SFA.DAS.Admin.Roatp.Web.Models;

public class SelectTrainingProviderViewModel : SelectTrainingProviderSubmitViewModel
{
    // this is a temporary field only needed to allow testing.  Will be removed in CSP-2210
    public string MatchedResult { get; set; } = string.Empty;
}

public class SelectTrainingProviderSubmitViewModel : IBackLink
{
    public string? SearchTerm
    {
        get => string.IsNullOrWhiteSpace(Ukprn) ? string.Empty : $"{LegalName}: ({Ukprn})";
    }

    public string? LegalName { get; set; }
    public string? Ukprn { get; set; }
}
