using CashFlow.Api.Models;
using static ViniBas.ResultPattern.ResultObjects.Error;

namespace CashFlow.Api.UnitTests;

public class EntryTests
{

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_ShouldAssignPropertiesCorrectly(bool provideOptionalArguments)
    {
        var expectedValue = 150.25m;
        var expectedType = EntryType.Credit;
        var expectedDescription = provideOptionalArguments ?
            "Constructor Test Description" : null;
        DateTime? transactionDateToProvide = provideOptionalArguments
            ? new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) : null;

        var entry = new Entry(expectedValue, expectedType, expectedDescription, transactionDateToProvide);

        entry.Id.Should().NotBeEmpty();
        entry.Value.Should().Be(expectedValue);
        entry.Type.Should().Be(expectedType);
        entry.Description.Should().Be(expectedDescription);
        entry.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        entry.TransactionAtUtc.Should().Be(provideOptionalArguments ? transactionDateToProvide : entry.CreatedAtUtc);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenValid()
    {
        var entry = new Entry(123.45m, EntryType.Credit, "Test Entry", DateTime.UtcNow);

        var result = entry.Validate();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenInvalid()
    {
        var entryNegative = new Entry(-1m, EntryType.Credit, "Test Entry", DateTime.UtcNow);
        var entryInvalidMoney = new Entry(12.345m, EntryType.Credit, LoremNET.Source.LoremIpsum[..250], null);
        var entryZeroAndBigDescription = new Entry(0, EntryType.Debit, LoremNET.Source.LoremIpsum[..251], null);

        var resultNegative = entryNegative.Validate();
        var resultInvalidMoney = entryInvalidMoney.Validate();
        var resultZeroAndBigDescription = entryZeroAndBigDescription.Validate();

        var allResults = new[] {
            resultNegative,
            resultInvalidMoney,
            resultZeroAndBigDescription,
        };
        foreach (var result in allResults)
        {
            result.IsSuccess.Should().BeFalse();
            result.Error.Type.Should().Be("Validation");
        }

        ErrorDetails[] negativeErrorDetails = [
            new ("ValueMustBeGreaterThanZero", "The entry value must be greater than zero.")
        ];
        ErrorDetails[] invalidMoneyDetails = [
            new ("ValueIsInAIncorrectFormat", "The monetary value entered is in an incorrect format.")
        ];
        ErrorDetails[] zeroAndBigDescriptionErrorDetails = [
            new ("ValueMustBeGreaterThanZero", "The entry value must be greater than zero."),
            new ("DescriptionIsGreatherThenMaxLength", "The entry description cannot be longer than 250 characters.")
        ];

        var allExpectDetails = new[] {
            (res: resultNegative, msgs: negativeErrorDetails),
            (res: resultInvalidMoney, msgs: invalidMoneyDetails),
            (res: resultZeroAndBigDescription, msgs: zeroAndBigDescriptionErrorDetails),
        };
        foreach (var (res, msgs) in allExpectDetails)
            res.Error.Details.Should().BeEquivalentTo(msgs);
    }
}
