using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.Filters;

[TestFixture]
public class FiltersViewModelTests
{
    [Test]
    public void ShowFilterOptions_WhenNoClearSections_ThenIsFalse()
    {
        var model = new FiltersViewModel { Route = "route" };

        model.ShowFilterOptions.Should().BeFalse();
    }

    [Test]
    public void ShowFilterOptions_WhenClearSectionsExist_ThenIsTrue()
    {
        var model = new FiltersViewModel
        {
            Route = "route",
            ClearFilterSections =
            [
                new ClearFilterSectionViewModel
                {
                    FilterType = FilterService.FilterType.ProviderName,
                    Title = "Provider name",
                    Items = [new ClearFilterItemViewModel { DisplayText = "Beacon", ClearLink = "/" }]
                }
            ]
        };

        model.ShowFilterOptions.Should().BeTrue();
    }
}
