namespace SFA.DAS.Admin.Roatp.Web.Models;

public class AddProviderRadioButtonSelectionModel
{
    public bool IsSelected { get; set; }
    public string Checked => IsSelected ? "checked" : "";
}
