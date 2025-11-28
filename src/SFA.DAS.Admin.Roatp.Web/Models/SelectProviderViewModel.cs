namespace SFA.DAS.Admin.Roatp.Web.Models;

public class SelectProviderViewModel : SelectProviderSubmitModel, IBackLink
{
}

public class SelectProviderSubmitModel
{
    public string? Ukprn { get; set; }
}