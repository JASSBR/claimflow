using ClaimFlow.SharedKernel;

namespace ClaimFlow.Claims.Domain.UnitTests;

public class ResultTests
{
    private static readonly Error SampleError = Error.Validation("sample", "Sample error.");

    [Fact]
    public void Value_Throws_OnFailure()
    {
        Result<int> result = SampleError;

        result.IsFailure.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromValue_IsSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Failure_RequiresAtLeastOneError()
    {
        Should.Throw<ArgumentException>(() => Result.Failure());
    }
}
