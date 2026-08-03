using SFA.DAS.Admin.Roatp.Web.Models.Filters.Abstract;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;

public class CheckboxListFilterSectionViewModel : FilterSection
{
    public List<FilterItemViewModel> Items { get; set; } = [];

    public CheckboxListFilterSectionViewModel()
    {
        FilterComponentType = FilterComponentType.CheckboxList;
    }
}
