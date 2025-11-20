using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class AddProviderValidator : AbstractValidator<AddProviderSubmitModel>
{
    public AddProviderValidator()
    {
        RuleFor(s => s.Ukprn!)
            .MustBeValidUkprnFormat();
    }
}