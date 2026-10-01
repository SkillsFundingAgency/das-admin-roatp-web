using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class ApplicationCacheServiceTests
{
    [Test]
    public async Task WhenGettingValue_AndKeyIsMissing_ThenReturnsDefault()
    {
        var sut = CreateSut();

        var result = await sut.GetAsync<string>("missing");

        result.Should().BeNull();
    }

    [Test]
    public async Task WhenSettingAndGettingValue_ThenReturnsCachedValue()
    {
        var sut = CreateSut();
        const string key = ApplicationCacheKeys.CoursesCacheKey;
        const string value = "Alpha course";

        await sut.SetAsync(key, value);
        var result = await sut.GetAsync<string>(key);

        result.Should().Be(value);
    }

    [Test]
    public async Task WhenSettingValue_ThenUsesFourHourExpiryByDefault()
    {
        var distributedCacheMock = new Mock<IDistributedCache>();
        DistributedCacheEntryOptions? capturedOptions = null;
        distributedCacheMock
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, _, options, _) =>
                capturedOptions = options)
            .Returns(Task.CompletedTask);
        var sut = new ApplicationCacheService(distributedCacheMock.Object);

        await sut.SetAsync("key", "value");

        capturedOptions.Should().NotBeNull();
        capturedOptions!.AbsoluteExpirationRelativeToNow.Should().Be(ApplicationCacheService.DefaultCacheDuration);
    }

    private static ApplicationCacheService CreateSut()
        => new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
}
