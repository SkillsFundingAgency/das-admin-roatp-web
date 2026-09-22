using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;

public class ModelStateExtensionsTests
{
    [Test]
    public void AddValidationError_ModelStateHasErrors_ShouldAddError()
    {
        var sut = new ModelStateDictionary();
        var firstErrorProp = "FirstError";
        var secondErrorProp = "SecondError";
        var firstErrorMsg = "First is required";
        var secondErrorMsg = "Second is required";
        var errors = new List<ValidationFailure>
        {
            new(firstErrorProp, firstErrorMsg),
            new(secondErrorProp, secondErrorMsg)
        };

        sut.AddValidationErrors(errors);

        sut.ContainsKey(firstErrorProp).Should().BeTrue();
        sut.ContainsKey(secondErrorProp).Should().BeTrue();
        sut[firstErrorProp]?.Errors[0].ErrorMessage.Should().Be(firstErrorMsg);
        sut[secondErrorProp]?.Errors[0].ErrorMessage.Should().Be(secondErrorMsg);
    }

    [Test]
    public void AddValidationError_ModelStateHasNoErrors_ShouldNotAddAnyErrors()
    {
        var sut = new ModelStateDictionary();
        var errors = new List<ValidationFailure>();

        sut.AddValidationErrors(errors);

        sut.Should().BeEmpty();
    }
}
