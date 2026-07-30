using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.Models.Filters.Abstract;

public abstract class FilterSection
{
    public required string Id { get; set; }
    public required string For { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string SubHeading { get; set; } = string.Empty;
    public FilterComponentType FilterComponentType { get; set; }
}
