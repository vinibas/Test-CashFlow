using Moq;
using Microsoft.AspNetCore.Http.HttpResults;
using CashFlow.Api.Endpoints;
using AwesomeAssertions;
using CashFlow.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.UnitTests.Endpoints;

public class EntryControlEndpointsTests
{
    private Mock<IEntryDao> _entryDaoMock;

    public EntryControlEndpointsTests()
    {
        _entryDaoMock = new Mock<IEntryDao>();
    }

    [Theory]
    [InlineData(1234.56, 'C')]
    [InlineData(7890.12, 'D')]
    public async Task EntryControlPostHandlerAsync_ValidEntry_ReturnsCreated(decimal value, char type)
    {
        const string description = "Some description";
        var requestModel = new Entry(value, type, description);
        SetupEntryDaoMockInsertAsync();

        var result = await EntryControlEndpoints.EntryControlPostHandlerAsync(_entryDaoMock.Object, requestModel);

        result.Should().BeOfType<Created>();
        VerifyEntryDaoMockIfInsertAsyncWasCalledOnce(value, type, description);
    }

    private void SetupEntryDaoMockInsertAsync() => _entryDaoMock.Setup(dao => dao.InsertAsync(It.IsAny<Entry>()));
    private void VerifyEntryDaoMockIfInsertAsyncWasCalledNever()
        => _entryDaoMock.Verify(dao => dao.InsertAsync(It.IsAny<Entry>()), Times.Never);
    private void VerifyEntryDaoMockIfInsertAsyncWasCalledOnce(decimal value, char type, string description)
        => _entryDaoMock.Verify(dao => dao.InsertAsync(It.Is<Entry>(e => EntryMatches(e, value, type, description))), Times.Once);
    private static bool EntryMatches(Entry entry, decimal value, char type, string description)
        => entry.Value == value && entry.Type == type && entry.Description == description;

    [Theory]
    [InlineData(0, 'C', "The entry value must be greater than zero.")]
    [InlineData(-1, 'D', "The entry value must be greater than zero.")]
    [InlineData(7890.12, 'E', "The entry type must be only 'C' for credit or 'D' for debit.")]
    public async Task EntryControlPostHandlerAsync_SendingInvalidField_ReturnsProblemDetails(decimal value, char type, string expectedError)
    {
        var requestModel = new Entry(value, type, null);
        SetupEntryDaoMockInsertAsync();

        var result = await EntryControlEndpoints.EntryControlPostHandlerAsync(_entryDaoMock.Object, requestModel);

        VerifyEntryDaoMockIfInsertAsyncWasCalledNever();

        var problemHttpResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemHttpResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemHttpResult.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        GetProblemDetailsErrors(problemHttpResult.ProblemDetails)
            .Should().BeEquivalentTo([expectedError]);
    }

    private static List<string> GetProblemDetailsErrors(ProblemDetails problemDetails)
        => problemDetails.Extensions.TryGetValue("errors", out var errors) && errors is not null ?
            (List<string>)errors : [];
}