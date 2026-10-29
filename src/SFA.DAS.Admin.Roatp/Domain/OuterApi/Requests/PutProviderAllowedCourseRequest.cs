namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;

public record PutProviderAllowedCourseRequest(DateTime? LastDateStarts, string UserId, string UserDisplayName);
