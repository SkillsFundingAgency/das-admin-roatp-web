using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models;
public class ProviderSummaryViewModelTests
{
    [Test, AutoData]
    public void MapModel_From_GetOrganisationResponse(
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
    public void MapModel_CompanyNumber_AsExpected(
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
    [InlineAutoData(ProviderType.Supporting, true)]
    [InlineAutoData(ProviderType.Employer, false)]
    [InlineAutoData(ProviderType.Main, false)]
    public void MapModel_IsSupportingProvider_AsExpected(
        ProviderType providerType,
        bool expected,
        GetOrganisationResponse response
    )
    {
        response.ProviderType = providerType!;
        var sut = (ProviderSummaryViewModel)response;
        sut.IsSupportingProvider.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(null, false)]
    [InlineAutoData("", false)]
    [InlineAutoData("trading name", true)]
    public void MapModel_ShowTradingName_AsExpected(
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
    public void MapModel_ShowCompanyNumber_AsExpected(
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
    public void MapModel_LastUpdatedDateText_ShowLastUpdatedDate_AsExpected(
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
    public void MapModel_ApplicationDeterminedDateText_AsExpected(
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
    public void MapModel_ShowRemovedReason_AsExpected(
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
    public void MapModel_RemovedDateText_AsExpected(
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
    public void MapModel_CharityNumberText_AsExpected(
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
    public void MapModel_OffersApprenticeshipsText_OffersShortCoursesText_AsExpected(
        bool offersApprenticeships,
        bool offersShortCourses,
        string expectedOffersApprenticeshipText,
        string expectedOffersShortCoursesText,
        GetOrganisationResponse response
    )
    {
        var allowedCourseTypes = new List<AllowedCourseType>();
        if (offersApprenticeships) allowedCourseTypes.Add(new AllowedCourseType { LearningType = LearningType.Standard, CourseTypeId = 1, CourseTypeName = "course type" });
        if (offersShortCourses) allowedCourseTypes.Add(new AllowedCourseType { LearningType = LearningType.ShortCourse, CourseTypeId = 1, CourseTypeName = "unit" });

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
    public void MapModel_CheckStatuses_AsExpected(
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
}
