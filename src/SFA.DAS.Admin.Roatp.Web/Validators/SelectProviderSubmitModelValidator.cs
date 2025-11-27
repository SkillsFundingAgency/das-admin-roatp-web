using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class SelectProviderSubmitModelValidator : AbstractValidator<AddProviderSubmitModel>
{
    public SelectProviderSubmitModelValidator()
    {
        RuleFor(s => s.Ukprn!)
            .MustBeValidUkprnFormat();
    }
}