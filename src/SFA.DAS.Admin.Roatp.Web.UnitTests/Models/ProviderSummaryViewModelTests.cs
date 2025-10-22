using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models;
public class ProviderSummaryViewModelTests
{
    [Test, AutoData]
    public void MapModel_From_EditOrganisationSessionModel(
        EditOrganisationSessionModel sessionModel
    )
    {
        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.Should().BeEquivalentTo(sessionModel, option => option
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
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.CompanyNumber = companyNumber!;
        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.CompanyNumber.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(ProviderType.Supporting, true)]
    [InlineAutoData(ProviderType.Employer, false)]
    [InlineAutoData(ProviderType.Main, false)]
    public void MapModel_IsSportinProvider_AsExpected(
        ProviderType providerType,
        bool expected,
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.ProviderType = providerType!;
        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.IsSupportingProvider.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(null, false)]
    [InlineAutoData("", false)]
    [InlineAutoData("trading name", true)]
    public void MapModel_ShowTradingName_AsExpected(
        string? tradingName,
        bool expected,
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.TradingName = tradingName!;
        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.ShowTradingName.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(null, false)]
    [InlineAutoData("", false)]
    [InlineAutoData("11111111", true)]
    public void MapModel_ShowCompanyNumber_AsExpected(
        string? companyNumber,
        bool expected,
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.CompanyNumber = companyNumber!;
        var sut = (ProviderSummaryViewModel)sessionModel;
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
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.LastUpdatedDate = null;
        if (day != null)
        {
            sessionModel.LastUpdatedDate = new DateTime(2024, 10, day.Value);
        }
        var sut = (ProviderSummaryViewModel)sessionModel;
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
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.ApplicationDeterminedDate = null;
        if (day != null)
        {
            sessionModel.ApplicationDeterminedDate = new DateTime(2024, 10, day.Value);
        }
        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.ApplicationDeterminedDateText.Should().Be(expectedDateText);
    }

    [Test]
    [InlineAutoData(1, true)]
    [InlineAutoData(null, false)]
    public void MapModel_ShowRemovedReason_AsExpected(
        int? reasonId,
       bool expected,
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.RemovedReasonId = null;
        if (reasonId != null)
        {
            sessionModel.RemovedReasonId = reasonId;
        }

        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.ShowRemovedReason.Should().Be(expected);
    }

    [Test]
    [InlineAutoData(10, "10 Oct 2024")]
    [InlineAutoData(1, "01 Oct 2024")]
    [InlineAutoData(null, "")]
    public void MapModel_RemovedDateText_AsExpected(
        int? day,
        string expectedDateText,
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.RemovedDate = null;
        if (day != null)
        {
            sessionModel.RemovedDate = new DateTime(2024, 10, day.Value);
        }
        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.RemovedDateText.Should().Be(expectedDateText);
    }

    [Test]
    [InlineAutoData("1234567", "1234567")]
    [InlineAutoData(null, "Not applicable")]
    [InlineAutoData("", "Not applicable")]
    public void MapModel_CharityNumberText_AsExpected(
        string charityNumber,
        string expected,
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.CharityNumber = charityNumber;
        var sut = (ProviderSummaryViewModel)sessionModel;
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
        EditOrganisationSessionModel sessionModel
    )
    {
        var allowedCourseTypes = new List<AllowedCourseType>();
        if (offersApprenticeships) allowedCourseTypes.Add(new AllowedCourseType { LearningType = LearningType.Standard, CourseTypeId = 1, CourseTypeName = "course type" });
        if (offersShortCourses) allowedCourseTypes.Add(new AllowedCourseType { LearningType = LearningType.ShortCourse, CourseTypeId = 1, CourseTypeName = "unit" });

        sessionModel.AllowedCourseTypes = allowedCourseTypes;

        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.OffersApprenticeshipsText.Should().Be(expectedOffersApprenticeshipText);
        sut.OffersShortCoursesText.Should().Be(expectedOffersShortCoursesText);
    }


    [Test, AutoData]
    public void MapModel_TypesOfShortCourses_AsExpected(
        EditOrganisationSessionModel sessionModel
    )
    {
        var firstItemExpected = "AAAA";
        var secondItemExpected = "DDD";
        var thirdItemExpected = "ZZZZ";

        var allowedCourseTypes = new List<AllowedCourseType>
        {
            new() { LearningType = LearningType.ShortCourse, CourseTypeId = 1, CourseTypeName = thirdItemExpected },
            new() { LearningType = LearningType.Standard, CourseTypeId = 2, CourseTypeName = "standard name" },
            new() { LearningType = LearningType.ShortCourse, CourseTypeId = 1, CourseTypeName = secondItemExpected },
            new() { LearningType = LearningType.ShortCourse, CourseTypeId = 1, CourseTypeName = firstItemExpected }
        };

        sessionModel.AllowedCourseTypes = allowedCourseTypes;

        var sut = (ProviderSummaryViewModel)sessionModel;
        var typesOfShortCourses = sut.TypesOfShortCourses;
        typesOfShortCourses.Count.Should().Be(3);
        typesOfShortCourses[0].Should().Be(firstItemExpected);
        typesOfShortCourses[1].Should().Be(secondItemExpected);
        typesOfShortCourses[2].Should().Be(thirdItemExpected);
    }

    [Test]
    [InlineAutoData(OrganisationStatus.Active, true, false, false, false)]
    public void MapModel_CheckStatuses_AsExpected(
        OrganisationStatus status,
        bool isActive,
        bool isActiveNoStarts,
        bool isOnboarding,
        bool isRemoved,
        EditOrganisationSessionModel sessionModel
    )
    {
        sessionModel.Status = status;
        var sut = (ProviderSummaryViewModel)sessionModel;
        sut.IsActive.Should().Be(isActive);
        sut.IsActiveNoStarts.Should().Be(isActiveNoStarts);
        sut.IsOnboarding.Should().Be(isOnboarding);
        sut.IsRemoved.Should().Be(isRemoved);
    }
}
