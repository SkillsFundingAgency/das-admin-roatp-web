using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class CacheServiceTests
{
    [Test, MoqAutoData]
    public void WhenGettingValueTwice_ThenReturnsCachedValue(
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        CacheService sut = new(httpContextAccessorMock.Object);

        sut.Set("cache", "key", "value");
        var found = sut.TryGetValue("cache", "key", out string? cached);

        found.Should().BeTrue();
        cached.Should().Be("value");
    }

    [Test, MoqAutoData]
    public void WhenHttpContextIsMissing_ThenThrows(
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessorMock)
    {
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        CacheService sut = new(httpContextAccessorMock.Object);

        var act = () => sut.Set("cache", "key", "value");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("HttpContext is not available.");
    }
}
