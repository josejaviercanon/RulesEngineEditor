using FluentAssertions;
using RulesEngine.Application.Validation;

namespace RulesEngine.Tests;

public sealed class JsonWorkflowSchemaValidatorTests
{
    private readonly JsonWorkflowSchemaValidator _validator = new();

    [Fact]
    public void Validate_WhenJsonIsEmpty_ReturnsRequiredError()
    {
        var result = _validator.Validate(string.Empty);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("RuleJson is required.");
        result.ResolvedVersion.Should().Be(1);
    }

    [Fact]
    public void Validate_WhenJsonIsInvalid_ReturnsParsingError()
    {
        var result = _validator.Validate("{not-json}");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("RuleJson must be valid JSON.");
    }

    [Fact]
    public void Validate_WhenSchemaVersionIsInvalid_ReturnsVersionError()
    {
        var result = _validator.Validate("{}", 0);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("SchemaVersion must be greater than 0.");
    }

    [Fact]
    public void Validate_WhenJsonIsValid_ReturnsSuccess()
    {
        var result = _validator.Validate("{}", 2);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.ResolvedVersion.Should().Be(2);
    }
}
