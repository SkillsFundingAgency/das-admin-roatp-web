using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models;
public class EditOrganisationSessionModelTests
{
    [Test, AutoData]
    public void MapModel_From_GetOrganisationResponse(
        GetOrganisationResponse getOrganisationResponse
    )
    {
        var sut = (EditOrganisationSessionModel)getOrganisationResponse;
        sut.Should().BeEquivalentTo(getOrganisationResponse);
    }
}
