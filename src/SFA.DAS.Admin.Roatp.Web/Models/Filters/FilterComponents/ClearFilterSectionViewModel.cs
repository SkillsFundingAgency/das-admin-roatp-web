using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;

public sealed class ClearFilterSectionViewModel
{
    public required FilterType FilterType { get; set; }
    public required string Title { get; set; }
    public List<ClearFilterItemViewModel> Items { get; set; } = [];
}
