using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Extensions;
public class ModelStateExtensionsTests
{
    [Test, MoqAutoData]
    public void AddValidationError_ModelStateHasErrors_ShouldAddError(
        ModelStateDictionary sut)
    {
        //Arrange
        var firstErrorProp = "FirstError";
        var secondErrorProp = "SecondError";
        var firstErrorMsg = "First is required";
        var secondErrorMsg = "Second is required";
        var errors = new List<ValidationFailure>
        {
            new ValidationFailure(firstErrorProp, firstErrorMsg),
            new ValidationFailure(secondErrorProp, secondErrorMsg)
        };

        //Act
        sut.AddValidationErrors(errors);

        //Assert
        sut.ContainsKey(firstErrorProp).Should().BeTrue();
        sut.ContainsKey(secondErrorProp).Should().BeTrue();
        sut[firstErrorProp]?.Errors[0].ErrorMessage.Should().Be(firstErrorMsg);
        sut[secondErrorProp]?.Errors[0].ErrorMessage.Should().Be(secondErrorMsg);
    }

    [Test, MoqAutoData]
    public void AddValidationError_ModelStateHasNoErrors_ShouldNotAddAnyErrors(
        ModelStateDictionary sut)
    {
        //Arrange
        var errors = new List<ValidationFailure>();

        //Act
        sut.AddValidationErrors(errors);

        //Assert
        sut.Should().BeEmpty();
    }
}