using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Services;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class FilterServiceTests
{
    private const string ClearFiltersBaseUrl = "/restricted-courses/105";

    [Test]
    public void CreateInputFilterSection_ThenReturnsTextBoxSectionWithValues()
    {
        var section = CreateInputFilterSection(
            "provider-name-input",
            ProviderNameSectionHeading,
            ProviderNameSectionSubHeading,
            nameof(FilterType.ProviderName),
            "Beacon");

        section.Should().BeOfType<TextBoxFilterSectionViewModel>();
        section.Id.Should().Be("provider-name-input");
        section.For.Should().Be(nameof(FilterType.ProviderName));
        section.Heading.Should().Be(ProviderNameSectionHeading);
        section.SubHeading.Should().Be(ProviderNameSectionSubHeading);
        section.FilterComponentType.Should().Be(FilterComponentType.TextBox);
        ((TextBoxFilterSectionViewModel)section).InputValue.Should().Be("Beacon");
    }

    [Test]
    public void CreateInputFilterSection_AndInputValueIsNull_ThenUsesEmptyString()
    {
        var section = (TextBoxFilterSectionViewModel)CreateInputFilterSection(
            "id",
            "heading",
            "subHeading",
            "for",
            null);

        section.InputValue.Should().BeEmpty();
    }

    [Test]
    public void CreateCheckboxListFilterSection_ThenReturnsCheckboxSectionWithItems()
    {
        var items = new List<FilterItemViewModel>
        {
            new() { Value = "OpenToNewStarts", DisplayText = "Open to new starts", IsSelected = true }
        };

        var section = CreateCheckboxListFilterSection(
            "delivery-status-filter",
            nameof(FilterType.DeliveryStatus),
            DeliveryStatusSectionHeading,
            "Hint",
            items);

        section.Should().BeOfType<CheckboxListFilterSectionViewModel>();
        section.Id.Should().Be("delivery-status-filter");
        section.For.Should().Be(nameof(FilterType.DeliveryStatus));
        section.Heading.Should().Be(DeliveryStatusSectionHeading);
        section.SubHeading.Should().Be("Hint");
        section.FilterComponentType.Should().Be(FilterComponentType.CheckboxList);
        ((CheckboxListFilterSectionViewModel)section).Items.Should().BeEquivalentTo(items);
    }

    [Test]
    public void CreateCheckboxListFilterSection_AndSubHeadingIsNull_ThenUsesEmptyString()
    {
        var section = CreateCheckboxListFilterSection(
            "id",
            "for",
            "heading",
            null,
            []);

        section.SubHeading.Should().BeEmpty();
    }

    [Test]
    public void CreateClearFilterSections_AndNoSelectedFilters_ThenReturnsEmpty()
    {
        var result = CreateClearFilterSections([], ClearFiltersBaseUrl);

        result.Should().BeEmpty();
    }

    [Test]
    public void CreateClearFilterSections_AndFilterHasNoValues_ThenSkipsSection()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.ProviderName] = []
        };

        var result = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl);

        result.Should().BeEmpty();
    }

    [Test]
    public void CreateClearFilterSections_AndSingleFilter_ThenClearLinkIsBaseUrl()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.ProviderName] = ["Beacon"]
        };

        var result = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl);

        result.Should().ContainSingle();
        result[0].Title.Should().Be(ProviderNameSectionHeading);
        result[0].FilterType.Should().Be(FilterType.ProviderName);
        result[0].Items.Single().DisplayText.Should().Be("Beacon");
        result[0].Items.Single().ClearLink.Should().Be(ClearFiltersBaseUrl);
    }

    [Test]
    public void CreateClearFilterSections_AndMultipleFilters_ThenClearLinkKeepsOtherFilters()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.ProviderName] = ["Beacon"],
            [FilterType.DeliveryStatus] = ["Open to new starts", "Closed to new starts"]
        };

        var overrideValueFunctions = new Dictionary<FilterType, Func<string, string>>
        {
            [FilterType.DeliveryStatus] = displayText => displayText switch
            {
                "Open to new starts" => "OpenToNewStarts",
                "Closed to new starts" => "ClosedToNewStarts",
                _ => displayText
            }
        };

        var result = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl, overrideValueFunctions);

        result.Should().HaveCount(2);

        var clearProviderLink = result
            .Single(section => section.FilterType == FilterType.ProviderName)
            .Items.Single().ClearLink;

        clearProviderLink.Should().Be(
            $"{ClearFiltersBaseUrl}?DeliveryStatus=OpenToNewStarts&DeliveryStatus=ClosedToNewStarts");

        var clearOpenLink = result
            .Single(section => section.FilterType == FilterType.DeliveryStatus)
            .Items.Single(item => item.DisplayText == "Open to new starts")
            .ClearLink;

        clearOpenLink.Should().Be($"{ClearFiltersBaseUrl}?ProviderName=Beacon&DeliveryStatus=ClosedToNewStarts");
    }

    [Test]
    public void CreateClearFilterSections_AndValueNeedsEncoding_ThenEncodesQueryValue()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.ProviderName] = ["Beacon & Co"],
            [FilterType.DeliveryStatus] = ["OpenToNewStarts"]
        };

        var clearDeliveryLink = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl)
            .Single(section => section.FilterType == FilterType.DeliveryStatus)
            .Items.Single().ClearLink;

        clearDeliveryLink.Should().Be($"{ClearFiltersBaseUrl}?ProviderName=Beacon%20%26%20Co");
    }

    [Test]
    public void AddSelectedFilter_WithSingleValue_ThenAddsWhenNotBlank()
    {
        var filters = new Dictionary<FilterType, IEnumerable<string>>();

        AddSelectedFilter(filters, FilterType.ProviderName, "  ");
        filters.Should().BeEmpty();

        AddSelectedFilter(filters, FilterType.ProviderName, "Beacon");
        filters[FilterType.ProviderName].Should().BeEquivalentTo(["Beacon"]);
    }

    [Test]
    public void AddSelectedFilter_WithMultipleValues_ThenAddsNonBlankValues()
    {
        var filters = new Dictionary<FilterType, IEnumerable<string>>();

        AddSelectedFilter(filters, FilterType.DeliveryStatus, ["", "  "]);
        filters.Should().BeEmpty();

        AddSelectedFilter(filters, FilterType.DeliveryStatus, ["Open", "", "Closed"]);
        filters[FilterType.DeliveryStatus].Should().BeEquivalentTo(["Open", "Closed"]);
    }
}
