using AwesomeAssertions;
using CashFlow.Api.Models;
using static ViniBas.ResultPattern.ResultObjects.Error;

namespace CashFlow.Api.UnitTests;

public class EntryTests
{
    [Fact]
    public void Validate_ShouldReturnSuccess_WhenValid()
    {
        var entry = new Entry(123.45m, 'C', "Test Entry");

        var result = entry.Validate();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenInvalid()
    {
        var entryNegative = new Entry(-1m, 'C', "Test Entry");
        var entryZeroAndType = new Entry(0, 'E', null);

        var resultNegative = entryNegative.Validate();
        var resultZeroAndType = entryZeroAndType.Validate();

        resultNegative.IsSuccess.Should().BeFalse();
        resultZeroAndType.IsSuccess.Should().BeFalse();

        resultNegative.Error.Type.Should().Be("Validation");
        ErrorDetails[] negativeErrorDetails = [ new ("ValueMustBeGreaterThanZero", "The entry value must be greater than zero.") ];
        resultNegative.Error.Details.Should().BeEquivalentTo(negativeErrorDetails);
        ErrorDetails[] zeroAndTypeErrorDetails = [
            new ("ValueMustBeGreaterThanZero", "The entry value must be greater than zero."),
            new ("TypesAreDifferentFromCAndD", "The entry type must be only 'C' for credit or 'D' for debit."),
        ];
        resultZeroAndType.Error.Details.Should().BeEquivalentTo(zeroAndTypeErrorDetails);
    }
}
