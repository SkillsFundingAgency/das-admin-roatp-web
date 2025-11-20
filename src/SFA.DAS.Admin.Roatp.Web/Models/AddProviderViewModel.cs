namespace SFA.DAS.Admin.Roatp.Web.Models;

public class AddProviderViewModel : AddProviderSubmitModel, IBackLink
{
}

public class AddProviderSubmitModel
{
    public string? Ukprn { get; set; }
}