using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.RestrictedCourses;

[TestFixture]
public class AllowedProviderItemViewModelTests
{
    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndNoLastStartDate_ThenMapsOpenToNewStarts(
        ProviderCourseModel provider)
    {
        provider.LastDateStarts = null;

        AllowedProviderItemViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
        model.DeliveryStatusDescription.Should().Be("Open to new starts");
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--green");
        model.HasLastStartDate.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndFutureLastStartDate_ThenMapsLastStartDateAdded(
        ProviderCourseModel provider)
    {
        var date = DateTime.UtcNow.Date.AddDays(10);
        provider.LastDateStarts = date;

        AllowedProviderItemViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
        model.DeliveryStatusDescription.Should().Be("Last start date added");
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--orange");
        model.HasLastStartDate.Should().BeTrue();
        model.LastStartDateText.Should().Be(date.ToString("dd MMM yyyy"));
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndPastLastStartDate_ThenMapsClosedToNewStarts(
        ProviderCourseModel provider)
    {
        var date = DateTime.UtcNow.Date.AddDays(-10);
        provider.LastDateStarts = date;

        AllowedProviderItemViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
        model.DeliveryStatusDescription.Should().Be("Closed to new starts");
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--grey");
        model.HasLastStartDate.Should().BeTrue();
        model.LastStartDateText.Should().Be(date.ToString("dd MMM yyyy"));
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndNoLastStartDate_ThenLastStartDateTextIsEmpty(
        ProviderCourseModel provider)
    {
        provider.LastDateStarts = null;

        AllowedProviderItemViewModel model = provider;

        model.LastStartDateText.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_ThenMapsUkprnAndProviderName(
        ProviderCourseModel provider)
    {
        AllowedProviderItemViewModel model = provider;

        model.Ukprn.Should().Be(provider.Ukprn);
        model.ProviderName.Should().Be(provider.ProviderName);
    }

    [Test]
    public void ChangeUrl_DefaultsToHash()
    {
        var model = new AllowedProviderItemViewModel
        {
            ProviderName = "Provider"
        };

        model.ChangeUrl.Should().Be("#");
    }
}
