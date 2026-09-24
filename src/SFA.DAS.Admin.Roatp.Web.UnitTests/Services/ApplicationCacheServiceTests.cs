using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Caching.Memory;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class ApplicationCacheServiceTests
{
    [Test]
    public void WhenSettingValue_ThenTryGetReturnsCachedValue()
    {
        var sut = new ApplicationCacheService(new MemoryCache(new MemoryCacheOptions()));

        sut.Set("key", "value");
        var found = sut.TryGet("key", out string? cached);

        using (new AssertionScope())
        {
            found.Should().BeTrue();
            cached.Should().Be("value");
        }
    }

    [Test]
    public void WhenKeyDoesNotExist_ThenTryGetReturnsFalse()
    {
        var sut = new ApplicationCacheService(new MemoryCache(new MemoryCacheOptions()));

        var found = sut.TryGet("missing", out string? cached);

        using (new AssertionScope())
        {
            found.Should().BeFalse();
            cached.Should().BeNull();
        }
    }
}
