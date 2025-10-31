namespace SFA.DAS.Admin.Roatp.Web.Models;

public class SelectTrainingProviderViewModel : IBackLink
{
    public string? SearchTerm
    {
        get => string.IsNullOrWhiteSpace(Ukprn) ? string.Empty : $"{LegalName}: ({Ukprn})";
    }

    public string? LegalName { get; set; }
    public string? Ukprn { get; set; }
}
