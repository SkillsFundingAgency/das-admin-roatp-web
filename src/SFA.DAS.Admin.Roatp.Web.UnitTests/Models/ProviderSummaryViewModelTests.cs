using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models;

public class ProviderSummaryViewModelTests
{
    [Test, AutoData]
    public void WhenMappingFromResponse_ThenMapsEquivalentProperties(
        GetOrganisationResponse response
    )
    {
        var sut = (ProviderSummaryViewModel)response;
        sut.Should().BeEquivalentTo(response, option => option
            .Excluding(o => o.OrganisationId)
            .Excluding(o => o.OrganisationId)
            .Excluding(o => o.CharityNumber)
            .Excluding(o => o.OrganisationTypeId)
            .Excluding(o => o.LastUpdatedDate)
            .Excluding(o => o.ApplicationDeterminedDate)
            .Excluding(o => o.RemovedReasonId)
            .Excluding(o => o.RemovedDate)
        );
    }

    [Test]
    [InlineAutoData(null, "Not applicable")]
    [InlineAutoData("12345678", "12345678")]
    public void WhenMappingFromResponse_ThenSetsCompanyNumber(
        string? companyNumber,
        string expected,
        GetOrganisationResponse response
    )
    {
        response.CompanyNumber = companyNumber!;
        var sut = (ProviderSummaryViewModel)response;
        sut.CompanyNumber.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(ProviderType.Employer, true)]
    [InlineAutoData(ProviderType.Supporting, false)]
    [InlineAutoData(ProviderType.Main, false)]
    public void WhenMappingFromResponse_ThenSetsIsEmployerProvider(
        ProviderType providerType,
        bool expected,
        GetOrganisationResponse response
    )
    {
        response.ProviderType = providerType!;
        var sut = (ProviderSummaryViewModel)response;
        sut.IsEmployerProvider.Should().Be(expected);
    }

    [Test, AutoData]
    public void WhenMappingFromResponse_AndProviderTypeIsEmployer_ThenDoesNotShowMainProviderCourseOffering(
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Employer;

        var sut = (ProviderSummaryViewModel)response;

        using (new AssertionScope())
        {
            sut.IsEmployerProvider.Should().BeTrue();
            sut.ShowManageCourseOffering.Should().BeFalse();
            sut.IsRestrictedMainProvider.Should().BeFalse();
        }
    }

    [Test]
    [InlineAutoData(null, false)]
    [InlineAutoData("", false)]
    [InlineAutoData("trading name", true)]
    public void WhenMappingFromResponse_ThenSetsShowTradingName(
        string? tradingName,
        bool expected,
        GetOrganisationResponse response
    )
    {
        response.TradingName = tradingName!;
        var sut = (ProviderSummaryViewModel)response;
        sut.ShowTradingName.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(null, false)]
    [InlineAutoData("", false)]
    [InlineAutoData("11111111", true)]
    public void WhenMappingFromResponse_ThenSetsShowCompanyNumber(
        string? companyNumber,
        bool expected,
        GetOrganisationResponse response
    )
    {
        response.CompanyNumber = companyNumber!;
        var sut = (ProviderSummaryViewModel)response;
        sut.ShowCompanyNumber.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(10, "10 Oct 2024", true)]
    [InlineAutoData(1, "01 Oct 2024", true)]
    [InlineAutoData(null, "", false)]
    public void WhenMappingFromResponse_ThenSetsLastUpdatedDate(
        int? day,
       string expectedDateText,
        bool expectedShowLastUpdatedDate,
        GetOrganisationResponse response
    )
    {
        response.LastUpdatedDate = null;
        if (day != null)
        {
            response.LastUpdatedDate = new DateTime(2024, 10, day.Value);
        }
        var sut = (ProviderSummaryViewModel)response;
        sut.LastUpdatedDateText.Should().Be(expectedDateText);
        sut.ShowLastUpdatedDate.Should().Be(expectedShowLastUpdatedDate);
    }

    [Test]
    [InlineAutoData(10, "10 Oct 2024")]
    [InlineAutoData(1, "01 Oct 2024")]
    [InlineAutoData(null, "Unavailable")]
    public void WhenMappingFromResponse_ThenSetsApplicationDeterminedDateText(
        int? day,
        string expectedDateText,
        GetOrganisationResponse response
    )
    {
        response.ApplicationDeterminedDate = null;
        if (day != null)
        {
            response.ApplicationDeterminedDate = new DateTime(2024, 10, day.Value);
        }
        var sut = (ProviderSummaryViewModel)response;
        sut.ApplicationDeterminedDateText.Should().Be(expectedDateText);
    }

    [Test]
    [InlineAutoData(1, true)]
    [InlineAutoData(null, false)]
    public void WhenMappingFromResponse_ThenSetsShowRemovedReason(
        int? reasonId,
       bool expected,
        GetOrganisationResponse response
    )
    {
        response.RemovedReasonId = null;
        if (reasonId != null)
        {
            response.RemovedReasonId = reasonId;
        }

        var sut = (ProviderSummaryViewModel)response;
        sut.ShowRemovedReason.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(10, "10 Oct 2024")]
    [InlineAutoData(1, "01 Oct 2024")]
    [InlineAutoData(null, "")]
    public void WhenMappingFromResponse_ThenSetsRemovedDateText(
        int? day,
        string expectedDateText,
        GetOrganisationResponse response
    )
    {
        response.RemovedDate = null;
        if (day != null)
        {
            response.RemovedDate = new DateTime(2024, 10, day.Value);
        }
        var sut = (ProviderSummaryViewModel)response;
        sut.RemovedDateText.Should().Be(expectedDateText);
    }

    [Test]
    [InlineAutoData("1234567", "1234567")]
    [InlineAutoData(null, "Not applicable")]
    [InlineAutoData("", "Not applicable")]
    public void WhenMappingFromResponse_ThenSetsCharityNumberText(
        string charityNumber,
        string expected,
        GetOrganisationResponse response
    )
    {
        response.CharityNumber = charityNumber;
        var sut = (ProviderSummaryViewModel)response;
        sut.CharityNumberText.Should().Be(expected);
    }


    [Test]
    [InlineAutoData(true, true, "Yes", "Yes")]
    [InlineAutoData(true, false, "Yes", "No")]
    [InlineAutoData(false, true, "No", "Yes")]
    [InlineAutoData(false, false, "No", "No")]
    public void WhenMappingFromResponse_ThenSetsOffersApprenticeshipsAndShortCoursesText(
        bool offersApprenticeships,
        bool offersShortCourses,
        string expectedOffersApprenticeshipText,
        string expectedOffersShortCoursesText,
        GetOrganisationResponse response
    )
    {
        var allowedCourseTypes = new List<AllowedCourseType>();
        if (offersApprenticeships) allowedCourseTypes.Add(new AllowedCourseType { CourseType = CourseType.Apprenticeship });
        if (offersShortCourses) allowedCourseTypes.Add(new AllowedCourseType { CourseType = CourseType.ShortCourse });

        response.AllowedCourseTypes = allowedCourseTypes;

        var sut = (ProviderSummaryViewModel)response;

        sut.OffersApprenticeshipsText.Should().Be(expectedOffersApprenticeshipText);
        sut.OffersShortCoursesText.Should().Be(expectedOffersShortCoursesText);
    }

    [Test]
    [InlineAutoData(OrganisationStatus.Active, true, false, false, false)]
    [InlineAutoData(OrganisationStatus.ActiveNoStarts, false, true, false, false)]
    [InlineAutoData(OrganisationStatus.OnBoarding, false, false, true, false)]
    [InlineAutoData(OrganisationStatus.Removed, false, false, false, true)]
    public void WhenMappingFromResponse_ThenSetsStatusFlags(
        OrganisationStatus status,
        bool isActive,
        bool isActiveNoStarts,
        bool isOnboarding,
        bool isRemoved,
        GetOrganisationResponse response
    )
    {
        response.Status = status;
        var sut = (ProviderSummaryViewModel)response;
        sut.IsActive.Should().Be(isActive);
        sut.IsActiveNoStarts.Should().Be(isActiveNoStarts);
        sut.IsOnboarding.Should().Be(isOnboarding);
        sut.IsRemoved.Should().Be(isRemoved);
    }

    [Test]
    [InlineAutoData(ProviderType.Main, false, true, false)]
    [InlineAutoData(ProviderType.Main, true, true, true)]
    [InlineAutoData(ProviderType.Employer, false, false, false)]
    [InlineAutoData(ProviderType.Supporting, false, false, false)]
    public void WhenMappingFromResponse_ThenSetsShowManageCourseOffering(
        ProviderType providerType,
        bool isRestricted,
        bool expectedShowManageCourseOffering,
        bool expectedIsRestrictedMainProvider,
        GetOrganisationResponse response)
    {
        response.ProviderType = providerType;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.Apprenticeship,
                IsRestricted = isRestricted
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        using (new AssertionScope())
        {
            sut.ShowManageCourseOffering.Should().Be(expectedShowManageCourseOffering);
            sut.IsRestrictedMainProvider.Should().Be(expectedIsRestrictedMainProvider);
        }
    }

    [Test, AutoData]
    public void WhenMappingFromResponse_AndMainProviderHasNoApprenticeshipCourseType_ThenShowManageCourseOfferingIsFalse(
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Main;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.ShortCourse,
                IsRestricted = false
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        sut.ShowManageCourseOffering.Should().BeFalse();
    }

    [Test]
    [InlineAutoData(16, 16)]
    [InlineAutoData(null, 0)]
    public void WhenMappingFromResponse_ThenSetsRestrictedCoursesCount(
        int? restrictedCount,
        int expected,
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Main;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.Apprenticeship,
                IsRestricted = false,
                RestrictedCount = restrictedCount
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        sut.RestrictedCoursesCount.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(8, 8)]
    [InlineAutoData(0, 0)]
    [InlineAutoData(null, 0)]
    public void WhenMappingFromResponse_ThenSetsApprovedCoursesCount(
        int? allowedCount,
        int expected,
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Main;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.Apprenticeship,
                IsRestricted = true,
                AllowedCount = allowedCount
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        using (new AssertionScope())
        {
            sut.ApprovedCoursesCount.Should().Be(expected);
            sut.ShowManageCourseOffering.Should().BeTrue();
            sut.IsRestrictedMainProvider.Should().BeTrue();
        }
    }

    [Test]
    [InlineAutoData(12, 12)]
    [InlineAutoData(0, 0)]
    [InlineAutoData(null, 0)]
    public void WhenMappingFromResponse_AndShortCoursesAreAllowed_ThenSetsApprovedApprenticeshipUnitsCount(
        int? allowedCount,
        int expected,
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Main;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.Apprenticeship,
                IsRestricted = false
            },
            new AllowedCourseType
            {
                CourseType = CourseType.ShortCourse,
                AllowedCount = allowedCount
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        using (new AssertionScope())
        {
            sut.ApprovedApprenticeshipUnitsCount.Should().Be(expected);
            sut.ShowManageCourseOffering.Should().BeTrue();
        }
    }

    [Test]
    [InlineAutoData(12, 12)]
    [InlineAutoData(0, 0)]
    [InlineAutoData(null, 0)]
    public void WhenMappingFromResponse_AndRestrictedMainProviderAndShortCoursesAreAllowed_ThenSetsApprovedApprenticeshipUnitsCount(
        int? allowedCount,
        int expected,
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Main;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.Apprenticeship,
                IsRestricted = true,
                AllowedCount = 8
            },
            new AllowedCourseType
            {
                CourseType = CourseType.ShortCourse,
                AllowedCount = allowedCount
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        using (new AssertionScope())
        {
            sut.ApprovedApprenticeshipUnitsCount.Should().Be(expected);
            sut.ShowManageCourseOffering.Should().BeTrue();
            sut.IsRestrictedMainProvider.Should().BeTrue();
        }
    }

    [Test, AutoData]
    public void WhenMappingFromResponse_AndShortCoursesAreNotAllowed_ThenApprovedApprenticeshipUnitsCountIsZero(
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Main;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.Apprenticeship,
                IsRestricted = false,
                RestrictedCount = 16
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        using (new AssertionScope())
        {
            sut.ApprovedApprenticeshipUnitsCount.Should().Be(0);
            sut.ShowManageCourseOffering.Should().BeTrue();
        }
    }

    [Test, AutoData]
    public void WhenMappingFromResponse_AndRestrictedMainProviderAndShortCoursesAreNotAllowed_ThenApprovedApprenticeshipUnitsCountIsZero(
        GetOrganisationResponse response)
    {
        response.ProviderType = ProviderType.Main;
        response.AllowedCourseTypes =
        [
            new AllowedCourseType
            {
                CourseType = CourseType.Apprenticeship,
                IsRestricted = true,
                AllowedCount = 8
            }
        ];

        var sut = (ProviderSummaryViewModel)response;

        using (new AssertionScope())
        {
            sut.ApprovedApprenticeshipUnitsCount.Should().Be(0);
            sut.ShowManageCourseOffering.Should().BeTrue();
            sut.IsRestrictedMainProvider.Should().BeTrue();
        }
    }
}
