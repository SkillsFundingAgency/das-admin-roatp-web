using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.Shared;

public sealed class WhenCreatingPaginationViewModel
{
    private const int PageSize = PaginationViewModel.DefaultPageSize;

    private Mock<IUrlHelper> _urlHelperMock = null!;

    [SetUp]
    public void SetUp()
    {
        _urlHelperMock = new Mock<IUrlHelper>();
        _urlHelperMock
            .Setup(x =>
                x.RouteUrl(
                    It.Is<UrlRouteContext>(c =>
                        c.RouteName!.Equals(RouteNames.RestrictedCourseDetails)
                    )
                )
            )
            .Returns("/restricted-courses/105");
    }

    [Test]
    public void Then_Page_Number_Should_Be_Set()
    {
        var sut = new PaginationViewModel(
            1,
            1,
            PageSize,
            _urlHelperMock.Object,
            RouteNames.RestrictedCourseDetails,
            []);

        sut.PageNumber.Should().Be(1);
    }

    [TestCase(1, 0)]
    [TestCase(1, 10)]
    [TestCase(3, 10)]
    public void Then_Returns_Empty_Model(int currentPage, int totalCount)
    {
        var sut = new PaginationViewModel(
            currentPage,
            totalCount,
            PageSize,
            _urlHelperMock.Object,
            RouteNames.RestrictedCourseDetails,
            []);

        sut.Pages.Should().BeEmpty();
    }

    [TestCase(1, 0, 0)]
    [TestCase(1, 10, 0)]
    [TestCase(1, 70, 0)]
    [TestCase(2, 20, 1)]
    [TestCase(7, 70, 6)]
    public void Then_Model_Adds_Previous_Link(int currentPage, int totalCount, int expectedPageNumberInPreviousLink)
    {
        var sut = new PaginationViewModel(
            currentPage,
            totalCount,
            PageSize,
            _urlHelperMock.Object,
            RouteNames.RestrictedCourseDetails,
            []);

        if (expectedPageNumberInPreviousLink > 0)
        {
            sut.Pages[0].Title.Should().Be(PaginationViewModel.PreviousPageTitle);
            sut.Pages[0].Url.Should().Contain($"PageNumber={expectedPageNumberInPreviousLink}");
        }
        else
        {
            sut.Pages.Should().NotContain(p => p.Title == PaginationViewModel.PreviousPageTitle);
        }
    }

    [TestCase(1, 0, 0)]
    [TestCase(1, 10, 0)]
    [TestCase(2, 20, 0)]
    [TestCase(1, 20, 2)]
    public void Then_Model_Adds_Next_Link(int currentPage, int totalCount, int expectedPageNumberInTheNextLink)
    {
        var sut = new PaginationViewModel(
            currentPage,
            totalCount,
            PageSize,
            _urlHelperMock.Object,
            RouteNames.RestrictedCourseDetails,
            []);

        if (expectedPageNumberInTheNextLink > 0)
        {
            var lastPage = sut.Pages.Count - 1;
            sut.Pages[lastPage].Title.Should().Be(PaginationViewModel.NextPageTitle);
            sut.Pages[lastPage].Url.Should().Contain($"PageNumber={expectedPageNumberInTheNextLink}");
        }
        else
        {
            sut.Pages.Should().NotContain(p => p.Title == PaginationViewModel.NextPageTitle);
        }
    }

    [Test]
    public void Then_Model_Sets_Correct_Page_Links()
    {
        var sut = new PaginationViewModel(
            2,
            30,
            PageSize,
            _urlHelperMock.Object,
            RouteNames.RestrictedCourseDetails,
            []);

        using (new AssertionScope())
        {
            sut.Pages.Should().HaveCount(5);
            sut.Pages[0].Title.Should().Be(PaginationViewModel.PreviousPageTitle);
            sut.Pages[0].Url.Should().Contain("PageNumber=1");
            sut.Pages[1].Title.Should().Be("1");
            sut.Pages[1].Url.Should().Contain("PageNumber=1");
            sut.Pages[2].Title.Should().Be("2");
            sut.Pages[2].Url.Should().BeNull();
            sut.Pages[3].Title.Should().Be("3");
            sut.Pages[3].Url.Should().Contain("PageNumber=3");
            sut.Pages[4].Title.Should().Be(PaginationViewModel.NextPageTitle);
            sut.Pages[4].Url.Should().Contain("PageNumber=3");
        }
    }

    [Test]
    public void When_Params_Do_Not_Contain_PageNumber_Then_PageNumber_Is_Added_And_Correct_Page_Links_Are_Set()
    {
        var sut = new PaginationViewModel(
            1,
            20,
            PageSize,
            _urlHelperMock.Object,
            RouteNames.RestrictedCourseDetails,
            []);

        using (new AssertionScope())
        {
            sut.Pages.Should().HaveCount(3);
            sut.Pages[1].Title.Should().Be("2");
            sut.Pages[1].Url.Should().Contain("PageNumber=2");
        }
    }

    [TestCase(1, 10, 0, 0)]
    [TestCase(2, 11, 2, 1)]
    [TestCase(3, 70, 6, 1)]
    [TestCase(4, 70, 6, 2)]
    [TestCase(9, 90, 6, 4)]
    public void Then_Model_Creates_Correct_Number_Of_Page_Links(
        int currentPage,
        int totalCount,
        int expectedPageLinksCount,
        int startPageNumber)
    {
        var sut = new PaginationViewModel(
            currentPage,
            totalCount,
            PageSize,
            _urlHelperMock.Object,
            RouteNames.RestrictedCourseDetails,
            []);

        sut.Pages.RemoveAll(p =>
            p.Title == PaginationViewModel.PreviousPageTitle
            || p.Title == PaginationViewModel.NextPageTitle);

        using (new AssertionScope())
        {
            sut.Pages.Should().HaveCount(expectedPageLinksCount);
            if (expectedPageLinksCount > 0)
            {
                var pageNumber = startPageNumber;
                for (var index = 0; index < expectedPageLinksCount; index++)
                {
                    sut.Pages[index].Title.Should().Be(pageNumber.ToString());
                    pageNumber++;
                }
            }
        }
    }

    [TestCase(1, 70, 10, 1, 6)]
    [TestCase(2, 70, 10, 1, 6)]
    [TestCase(3, 70, 10, 1, 6)]
    [TestCase(5, 70, 10, 2, 7)]
    [TestCase(6, 70, 10, 2, 7)]
    [TestCase(7, 70, 10, 2, 7)]
    [TestCase(7, 80, 10, 3, 8)]
    [TestCase(20, 100, 10, 5, 10)]
    public void Then_Get_Page_Range_Adjusts_Correctly(
        int currentPage,
        int totalRecords,
        int pageSize,
        int expectedStartPage,
        int expectedEndPage)
    {
        var (startPage, endPage) = PaginationViewModel.GetPageRange(currentPage, totalRecords, pageSize);

        using (new AssertionScope())
        {
            startPage.Should().Be(expectedStartPage);
            endPage.Should().Be(expectedEndPage);
        }
    }

    [Test]
    public void When_Url_Is_Set_Then_Page_Link_Has_Link_Is_True()
    {
        var pageLink = new PageLink("Title", "//dummyurl");

        pageLink.HasLink.Should().BeTrue();
    }

    [Test]
    public void When_Url_Is_Not_Set_Then_Page_Link_Has_Link_Is_False()
    {
        var pageLink = new PageLink("Title", string.Empty);

        pageLink.HasLink.Should().BeFalse();
    }
}
