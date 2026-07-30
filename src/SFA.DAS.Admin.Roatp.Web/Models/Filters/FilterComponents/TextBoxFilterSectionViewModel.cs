using SFA.DAS.Admin.Roatp.Web.Models.Filters.Abstract;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;

public sealed class TextBoxFilterSectionViewModel : FilterSection
{
    public string InputValue { get; set; } = string.Empty;

    public TextBoxFilterSectionViewModel()
    {
        FilterComponentType = FilterComponentType.TextBox;
    }
}
