using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public record AdminRoatpOuterApiConfiguration(string ApiBaseUrl, string SubscriptionKey, string ApiVersion) : IApimClientConfiguration;
