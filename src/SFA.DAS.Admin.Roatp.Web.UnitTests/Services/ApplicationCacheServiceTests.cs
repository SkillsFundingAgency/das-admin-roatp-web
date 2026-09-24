using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

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

    [Test]
    public void WhenCachedValueIsNull_ThenTryGetReturnsFalse()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCache.Set<string?>("key", null);
        var sut = new ApplicationCacheService(memoryCache);

        var found = sut.TryGet("key", out string? cached);

        using (new AssertionScope())
        {
            found.Should().BeFalse();
            cached.Should().BeNull();
        }
    }

    [Test, MoqAutoData]
    public void WhenSettingValue_WithoutExpiration_ThenDefaultExpirationIsApplied(
        [Frozen] Mock<IMemoryCache> memoryCacheMock,
        string key,
        string value)
    {
        var cacheEntry = Mock.Of<ICacheEntry>();
        memoryCacheMock.Setup(m => m.CreateEntry(key)).Returns(cacheEntry);
        var sut = new ApplicationCacheService(memoryCacheMock.Object);

        sut.Set(key, value);

        cacheEntry.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(ApplicationCacheService.DefaultExpirationMinutes));
    }

    [Test, MoqAutoData]
    public void WhenSettingValue_WithCustomExpiration_ThenCustomExpirationIsApplied(
        [Frozen] Mock<IMemoryCache> memoryCacheMock,
        string key,
        string value,
        TimeSpan expiration)
    {
        var cacheEntry = Mock.Of<ICacheEntry>();
        memoryCacheMock.Setup(m => m.CreateEntry(key)).Returns(cacheEntry);
        var sut = new ApplicationCacheService(memoryCacheMock.Object);

        sut.Set(key, value, expiration);

        cacheEntry.AbsoluteExpirationRelativeToNow.Should().Be(expiration);
    }

    [Test]
    public void WhenSettingValue_ThatAlreadyExists_ThenValueIsOverwritten()
    {
        var sut = new ApplicationCacheService(new MemoryCache(new MemoryCacheOptions()));

        sut.Set("key", "original");
        sut.Set("key", "updated");
        var found = sut.TryGet("key", out string? cached);

        using (new AssertionScope())
        {
            found.Should().BeTrue();
            cached.Should().Be("updated");
        }
    }
}
