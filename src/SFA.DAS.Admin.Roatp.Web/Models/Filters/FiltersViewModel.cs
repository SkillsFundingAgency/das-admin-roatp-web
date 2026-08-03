using SFA.DAS.Admin.Roatp.Web.Models.Filters.Abstract;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;

namespace SFA.DAS.Admin.Roatp.Web.Models.Filters;

public sealed class FiltersViewModel
{
    public required string Route { get; set; }
    public string? LarsCode { get; set; }
    public IReadOnlyList<FilterSection> FilterSections { get; set; } = [];
    public IReadOnlyList<ClearFilterSectionViewModel> ClearFilterSections { get; set; } = [];
    public bool ShowFilterOptions => ClearFilterSections.Count > 0;
}
