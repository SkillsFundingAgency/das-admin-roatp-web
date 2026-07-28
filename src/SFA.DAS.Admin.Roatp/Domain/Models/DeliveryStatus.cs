using System.ComponentModel;

namespace SFA.DAS.Admin.Roatp.Domain.Models;

public enum DeliveryStatus
{
    [Description("Open to new starts")]
    OpenToNewStarts,

    [Description("Last start date added")]
    LastStartDateAdded,

    [Description("Closed to new starts")]
    ClosedToNewStarts
}
