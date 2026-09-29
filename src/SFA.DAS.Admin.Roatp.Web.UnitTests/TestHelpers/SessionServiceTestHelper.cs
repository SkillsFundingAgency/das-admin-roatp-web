using Moq;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;

public static class SessionServiceTestHelper
{
    public static void SetupProviderName(this Mock<ISessionService> sessionServiceMock, int ukprn, string providerName)
    {
        sessionServiceMock
            .Setup(s => s.Get<Dictionary<int, string>>(SessionKeys.ProviderName))
            .Returns(new Dictionary<int, string> { [ukprn] = providerName });
    }

    public static void SetupProviderNameMissing(this Mock<ISessionService> sessionServiceMock)
    {
        sessionServiceMock
            .Setup(s => s.Get<Dictionary<int, string>>(SessionKeys.ProviderName))
            .Returns((Dictionary<int, string>?)null);
    }
}
