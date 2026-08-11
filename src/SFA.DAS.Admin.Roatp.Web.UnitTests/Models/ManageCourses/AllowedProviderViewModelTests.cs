using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class AllowedProviderViewModelTests
{
    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndNoLastDateStarts_ThenMapsOpenToNewStarts(
        ProviderCourseModel provider)
    {
        provider.LastDateStarts = null;

        AllowedProviderViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
        model.DeliveryStatusDescription.Should().Be("Open to new starts");
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--green");
        model.HasLastDateStarts.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndFutureLastDateStarts_ThenMapsLastStartDateAdded(
        ProviderCourseModel provider)
    {
        var date = DateTime.UtcNow.Date.AddDays(10);
        provider.LastDateStarts = date;

        AllowedProviderViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
        model.DeliveryStatusDescription.Should().Be("Last start date added");
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--orange");
        model.HasLastDateStarts.Should().BeTrue();
        model.LastDateStartsText.Should().Be(date.ToString("dd MMM yyyy"));
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndPastLastDateStarts_ThenMapsClosedToNewStarts(
        ProviderCourseModel provider)
    {
        var date = DateTime.UtcNow.Date.AddDays(-10);
        provider.LastDateStarts = date;

        AllowedProviderViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
        model.DeliveryStatusDescription.Should().Be("Closed to new starts");
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--grey");
        model.HasLastDateStarts.Should().BeTrue();
        model.LastDateStartsText.Should().Be(date.ToString("dd MMM yyyy"));
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_AndNoLastDateStarts_ThenLastDateStartsTextIsEmpty(
        ProviderCourseModel provider)
    {
        provider.LastDateStarts = null;

        AllowedProviderViewModel model = provider;

        model.LastDateStartsText.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public void WhenConvertingProvider_ThenMapsUkprnAndProviderName(
        ProviderCourseModel provider)
    {
        AllowedProviderViewModel model = provider;

        model.Ukprn.Should().Be(provider.Ukprn);
        model.ProviderName.Should().Be(provider.ProviderName);
    }

    [Test]
    public void ChangeUrl_DefaultsToHash()
    {
        var model = new AllowedProviderViewModel
        {
            ProviderName = "Provider"
        };

        model.ChangeUrl.Should().Be("#");
    }
}
