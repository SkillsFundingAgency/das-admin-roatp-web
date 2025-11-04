namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class RadioButtonSelectionModel
{
    public bool IsSelected { get; set; }
    public string Checked => IsSelected ? "checked" : "";
}