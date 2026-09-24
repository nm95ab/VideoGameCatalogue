using FluentAssertions;
using VideoGameCatalogue.Domain.Common;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ShouldHaveIsSuccessTrueAndNoError()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldHaveIsSuccessFalseAndExpectedError()
    {
        var error = Error.Validation("Code", "Description");
        var result = Result.Failure(error);
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void TypedResult_Success_ShouldReturnExpectedValue()
    {
        var result = Result<int>.Success(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TypedResult_AccessingValueOfFailure_ShouldThrowInvalidOperationException()
    {
        var result = Result<int>.Failure(Error.Validation("Code", "Fail"));
        var act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitOperator_ShouldCreateSuccessResult()
    {
        Result<string> result = "test";
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    [Fact]
    public void ConflictError_ShouldCreateConflictError()
    {
        var error = Error.Conflict("Code", "Description");
        error.Code.Should().Be("Code");
        error.Description.Should().Be("Description");
    }
}
