using Moq;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;

public static class SessionServiceTestHelper
{
    public static void SetupProviderName(this Mock<ISessionService> sessionServiceMock, int ukprn, string providerName)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderNameSessionModel>(SessionKeys.ProviderName(ukprn)))
            .Returns(new ProviderNameSessionModel
            {
                Ukprn = ukprn,
                ProviderName = providerName
            });
    }

    public static void SetupProviderNameMissing(this Mock<ISessionService> sessionServiceMock, int ukprn)
    {
        sessionServiceMock
            .Setup(s => s.Get<ProviderNameSessionModel>(SessionKeys.ProviderName(ukprn)))
            .Returns((ProviderNameSessionModel?)null);
    }
}
