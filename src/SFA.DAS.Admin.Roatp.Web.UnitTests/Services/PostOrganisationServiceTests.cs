using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;
using System.Security.Claims;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;
public class PostOrganisationServiceTests
{
    [Test, MoqAutoData]
    public async Task Test(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IHttpContextAccessor> context,
        AddProviderSessionModel addProviderSession,
        CancellationToken cancellationToken)
    {
        string userId = "test";
        string userGivenName = "user";
        string userSurname = "name";

        addProviderSession.ProviderTypeId = 1;

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Upn, userId),
                new Claim(ClaimTypes.GivenName, userGivenName),
                new Claim(ClaimTypes.Surname, userSurname)
            },
            "mock"));

        context.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        outerApiClientMock.Setup(x => x.PostOrganisation(It.IsAny<PostOrganisationCommand>(), cancellationToken)).Returns(Task.CompletedTask);

        PostOrganisationService sut = new(outerApiClientMock.Object, context.Object);

        await sut.PostOrganisation(addProviderSession, cancellationToken);

        outerApiClientMock.Verify(x => x.PostOrganisation(It.Is<PostOrganisationCommand>(c =>
        c.Ukprn == addProviderSession.Ukprn &&
        c.LegalName == addProviderSession.LegalName &&
        c.TradingName == addProviderSession.TradingName &&
        c.CompanyNumber == addProviderSession.CompanyNumber &&
        c.CharityNumber == addProviderSession.CharityNumber &&
        c.ProviderType == (ProviderType)addProviderSession.ProviderTypeId! &&
        c.OrganisationTypeId == addProviderSession.OrganisationTypeId &&
        c.DeliversApprenticeships == addProviderSession.OffersApprenticeships &&
        c.DeliversApprenticeshipUnits == addProviderSession.OffersApprenticeshipUnits &&
        c.RequestingUserId == userId &&
        c.RequestingUserDisplayName == userGivenName + " " + userSurname), cancellationToken), Times.Once);
    }
}