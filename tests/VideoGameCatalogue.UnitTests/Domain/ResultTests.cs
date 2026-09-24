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

    [Fact]
    public void NotFoundError_ShouldCreateExpectedError()
    {
        var error = Error.NotFound("NotFound.Code", "Item not found");
        error.Code.Should().Be("NotFound.Code");
        error.Description.Should().Be("Item not found");
    }

    private sealed class TestResult(bool isSuccess, Error error) : Result(isSuccess, error);

    [Fact]
    public void Constructor_WhenSuccessWithNonNoneError_ShouldThrowInvalidOperationException()
    {
        var act = () => new TestResult(true, Error.Validation("Err", "Desc"));
        act.Should().Throw<InvalidOperationException>().WithMessage("*Success result cannot contain an error.*");
    }

    [Fact]
    public void Constructor_WhenFailureWithNoneError_ShouldThrowInvalidOperationException()
    {
        var act = () => new TestResult(false, Error.None);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Failure result must contain an error.*");
    }
}
