using FluentAssertions;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class FilterServiceTests
{
    private const string LarsCode = "105";
    private const string ClearFiltersBaseUrl = "/restricted-courses/" + LarsCode;


    [Test]
    public void CreateInputFilterSection_ThenReturnsTextBoxSectionWithValues()
    {
        var section = CreateInputFilterSection(
            "search-term-input",
            SearchTermSectionHeading,
            SearchTermSectionSubHeading,
            nameof(FilterType.SearchTerm),
            "Beacon");

        section.Should().BeOfType<TextBoxFilterSectionViewModel>();
        section.Id.Should().Be("search-term-input");
        section.For.Should().Be(nameof(FilterType.SearchTerm));
        section.Heading.Should().Be(SearchTermSectionHeading);
        section.SubHeading.Should().Be(SearchTermSectionSubHeading);
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
            [FilterType.SearchTerm] = []
        };

        var result = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl);

        result.Should().BeEmpty();
    }

    [Test]
    public void CreateClearFilterSections_AndSingleFilter_ThenClearLinkIsBaseUrl()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.SearchTerm] = ["Beacon"]
        };

        var result = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl);

        result.Should().ContainSingle();
        result[0].Title.Should().Be(SearchTermSectionHeading);
        result[0].FilterType.Should().Be(FilterType.SearchTerm);
        result[0].Items.Single().DisplayText.Should().Be("Beacon");
        result[0].Items.Single().ClearLink.Should().Be(ClearFiltersBaseUrl);
    }

    [Test]
    public void CreateClearFilterSections_AndMultipleFilters_ThenClearLinkKeepsOtherFilters()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.SearchTerm] = ["Beacon"],
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
            .Single(section => section.FilterType == FilterType.SearchTerm)
            .Items.Single().ClearLink;

        clearProviderLink.Should().Be(
            $"{ClearFiltersBaseUrl}?DeliveryStatus=OpenToNewStarts&DeliveryStatus=ClosedToNewStarts");

        var clearOpenLink = result
            .Single(section => section.FilterType == FilterType.DeliveryStatus)
            .Items.Single(item => item.DisplayText == "Open to new starts")
            .ClearLink;

        clearOpenLink.Should().Be($"{ClearFiltersBaseUrl}?SearchTerm=Beacon&DeliveryStatus=ClosedToNewStarts");
    }

    [Test]
    public void CreateClearFilterSections_AndValueNeedsEncoding_ThenEncodesQueryValue()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.SearchTerm] = ["Beacon & Co"],
            [FilterType.DeliveryStatus] = ["OpenToNewStarts"]
        };

        var clearDeliveryLink = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl)
            .Single(section => section.FilterType == FilterType.DeliveryStatus)
            .Items.Single().ClearLink;

        clearDeliveryLink.Should().Be($"{ClearFiltersBaseUrl}?SearchTerm=Beacon%20%26%20Co");
    }

    [Test]
    public void CreateClearFilterSections_AndSelectedFiltersIncludesEmptyFilter_ThenIgnoresEmptyFilterInClearLinks()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.SearchTerm] = ["Beacon"],
            [FilterType.DeliveryStatus] = []
        };

        var result = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl);

        result.Should().ContainSingle();
        result[0].FilterType.Should().Be(FilterType.SearchTerm);
        result[0].Items.Single().ClearLink.Should().Be(ClearFiltersBaseUrl);
    }

    [Test]
    public void CreateClearFilterSections_AndSelectedFiltersIncludesWhitespace_ThenOmitsWhitespaceFromClearLink()
    {
        var selectedFilters = new Dictionary<FilterType, IEnumerable<string>>
        {
            [FilterType.SearchTerm] = ["Beacon"],
            [FilterType.DeliveryStatus] = ["OpenToNewStarts", "   "]
        };

        var clearProviderLink = CreateClearFilterSections(selectedFilters, ClearFiltersBaseUrl)
            .Single(section => section.FilterType == FilterType.SearchTerm)
            .Items.Single().ClearLink;

        clearProviderLink.Should().Be($"{ClearFiltersBaseUrl}?DeliveryStatus=OpenToNewStarts");
    }

    [Test]
    public void AddSelectedFilter_WithSingleValueAndValueIsBlank_ThenDoesNotAdd()
    {
        var filters = new Dictionary<FilterType, IEnumerable<string>>();

        AddSelectedFilter(filters, FilterType.SearchTerm, "  ");

        filters.Should().BeEmpty();
    }

    [Test]
    public void AddSelectedFilter_WithSingleValueAndValueIsNotBlank_ThenAddsValue()
    {
        var filters = new Dictionary<FilterType, IEnumerable<string>>();

        AddSelectedFilter(filters, FilterType.SearchTerm, "Beacon");

        filters[FilterType.SearchTerm].Should().BeEquivalentTo(["Beacon"]);
    }

    [Test]
    public void AddSelectedFilter_WithMultipleValues_AndAllValuesAreBlank_ThenDoesNotAdd()
    {
        var filters = new Dictionary<FilterType, IEnumerable<string>>();

        AddSelectedFilter(filters, FilterType.DeliveryStatus, ["", "  "]);

        filters.Should().BeEmpty();
    }

    [Test]
    public void AddSelectedFilter_WithMultipleValuesAndSomeValuesAreBlank_ThenAddsNonBlankValues()
    {
        var filters = new Dictionary<FilterType, IEnumerable<string>>();

        AddSelectedFilter(filters, FilterType.DeliveryStatus, ["Open", "", "Closed"]);

        filters[FilterType.DeliveryStatus].Should().BeEquivalentTo(["Open", "Closed"]);
    }
}
