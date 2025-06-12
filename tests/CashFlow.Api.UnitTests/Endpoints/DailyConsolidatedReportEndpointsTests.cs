using CashFlow.Api.Data.Daos;
using CashFlow.Api.Endpoints;
using CashFlow.Api.Endpoints.ViewModels;
using CashFlow.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.UnitTests.Endpoints;

public class DailyConsolidatedReportEndpointsTests
{
    private Mock<IDailyConsolidatedDao> _dailyConsolidatedDaoMock;
    private Mock<IEntryDao> _entryDaoMock;

    public DailyConsolidatedReportEndpointsTests()
    {
        _dailyConsolidatedDaoMock = new Mock<IDailyConsolidatedDao>();
        _entryDaoMock = new Mock<IEntryDao>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DailyConsolidatedReportHandlerAsync_ValidDateWithEntries_ReturnsValuesOnReport(bool isClosed)
    {
        var consultationDate = isClosed ? new DateOnly(2025, 05, 31) : DateOnly.FromDateTime(DateTime.UtcNow);
        var consolidatedMock = new DailyConsolidated(consultationDate, 700, 299);
        _dailyConsolidatedDaoMock.Setup(dao =>
            dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)))
            .ReturnsAsync(consolidatedMock);

        var result = await DailyConsolidatedReportEndpoints.DailyConsolidatedReportGetHandlerAsync
            (_dailyConsolidatedDaoMock.Object, consultationDate.ToString());

        var consolidatedToReturn = new DailyConsolidatedReportVM(consultationDate, 700, 299, 401, isClosed);

        var resultType = result.Should().BeOfType<Results<Ok<DailyConsolidatedReportVM>, ProblemHttpResult>>().Subject;
        var resultOk = resultType.Result.Should().BeOfType<Ok<DailyConsolidatedReportVM>>().Subject;
        resultOk.Value.Should().Be(consolidatedToReturn);
        _dailyConsolidatedDaoMock.Verify(dao => dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DailyConsolidatedReportHandlerAsync_ValidDateWithNoEntries_ReturnsZeroValuesOnReport(bool isClosed)
    {
        var consultationDate = isClosed ? new DateOnly(2025, 05, 31) : DateOnly.FromDateTime(DateTime.UtcNow);
        _dailyConsolidatedDaoMock.Setup(dao =>
            dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)))
            .ReturnsAsync(null as DailyConsolidated);

        var result = await DailyConsolidatedReportEndpoints.DailyConsolidatedReportGetHandlerAsync
            (_dailyConsolidatedDaoMock.Object, consultationDate.ToString());

        var consolidatedToReturn = new DailyConsolidatedReportVM(consultationDate, 0, 0, 0, isClosed);

        var resultType = result.Should().BeOfType<Results<Ok<DailyConsolidatedReportVM>, ProblemHttpResult>>().Subject;
        var resultOk = resultType.Result.Should().BeOfType<Ok<DailyConsolidatedReportVM>>().Subject;
        resultOk.Value.Should().Be(consolidatedToReturn);
        _dailyConsolidatedDaoMock.Verify(dao => dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)), Times.Once);
    }

    [Fact]
    public async Task DailyConsolidatedReportHandlerAsync_InvalidDate_ReturnsErrorMessage()
    {
        var consultationDate = "invalid_date";

        var result = await DailyConsolidatedReportEndpoints.DailyConsolidatedReportGetHandlerAsync
            (_dailyConsolidatedDaoMock.Object, consultationDate);

        var resultType = result.Should().BeOfType<Results<Ok<DailyConsolidatedReportVM>, ProblemHttpResult>>().Subject;
        var problemHttpResult = resultType.Result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemHttpResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemHttpResult.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        GetProblemDetailsErrors(problemHttpResult.ProblemDetails)
            .Should().BeEquivalentTo(["The date format is invalid. Use 'yyyy-MM-dd'."]);

        _dailyConsolidatedDaoMock.Verify(dao => dao.GetConsolidatedUpdatedAsync(It.IsAny<DateOnly>()), Times.Never);
    }

    private static List<string> GetProblemDetailsErrors(ProblemDetails problemDetails)
        => problemDetails.Extensions.TryGetValue("errors", out var errors) && errors is not null ?
            (List<string>)errors : [];
            
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DailyConsolidatedReportExtendedHandlerAsync_ValidDateWithEntries_ReturnsValuesOnReport(bool isClosed)
    {
        var consultationDate = isClosed ? new DateOnly(2025, 05, 31) : DateOnly.FromDateTime(DateTime.UtcNow);
        Entry[] entriesMock =
        [
            new Entry(10, EntryType.Credit, "test1", consultationDate.ToDateTime(new TimeOnly(9, 15))),
            new Entry(15.30m, EntryType.Debit, "test2", consultationDate.ToDateTime(new TimeOnly(9, 15))),
            new Entry(20.80m, EntryType.Credit, "test2", consultationDate.ToDateTime(new TimeOnly(9, 15))),
        ];
        var consolidatedMock = new DailyConsolidated(consultationDate, 30.80m, 15.30m);

        _entryDaoMock.Setup(dao =>
            dao.ListEntriesByDateAsync(It.Is<DateOnly>(d => d == consultationDate), It.IsAny<long>()))
            .ReturnsAsync(entriesMock);
        _dailyConsolidatedDaoMock.Setup(dao =>
            dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)))
            .ReturnsAsync(consolidatedMock);

        var result = await DailyConsolidatedReportEndpoints.DailyConsolidatedReportExtendedGetHandlerAsync
            (_dailyConsolidatedDaoMock.Object, _entryDaoMock.Object, consultationDate.ToString());

        var entriesVM = entriesMock.Select(e => new EntryReportItemVM(e.Value, (char)e.Type, e.Description, e.TransactionAtUtc));
        var consolidatedToReturn = new DailyConsolidatedReportExtendedVM(consultationDate, entriesVM, 30.80m, 15.30m, 15.50m, isClosed);

        var resultType = result.Should().BeOfType<Results<Ok<DailyConsolidatedReportExtendedVM>, ProblemHttpResult>>().Subject;
        var resultOk = resultType.Result.Should().BeOfType<Ok<DailyConsolidatedReportExtendedVM>>().Subject;
        resultOk.Value.Should().BeEquivalentTo(consolidatedToReturn);
        _entryDaoMock.Verify(dao => dao.ListEntriesByDateAsync(It.Is<DateOnly>(d => d == consultationDate), It.IsAny<long>()), Times.Once);
        _dailyConsolidatedDaoMock.Verify(dao => dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DailyConsolidatedReportExtendedHandlerAsync_ValidDateWithNoEntries_ReturnsZeroValuesOnReport(bool isClosed)
    {
        var consultationDate = isClosed ? new DateOnly(2025, 05, 31) : DateOnly.FromDateTime(DateTime.UtcNow);
        _dailyConsolidatedDaoMock.Setup(dao =>
            dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)))
            .ReturnsAsync(null as DailyConsolidated);

        var result = await DailyConsolidatedReportEndpoints.DailyConsolidatedReportExtendedGetHandlerAsync
            (_dailyConsolidatedDaoMock.Object, _entryDaoMock.Object, consultationDate.ToString());

        var consolidatedToReturn = new DailyConsolidatedReportExtendedVM(consultationDate, [], 0, 0, 0, isClosed);

        var resultType = result.Should().BeOfType<Results<Ok<DailyConsolidatedReportExtendedVM>, ProblemHttpResult>>().Subject;
        var resultOk = resultType.Result.Should().BeOfType<Ok<DailyConsolidatedReportExtendedVM>>().Subject;
        resultOk.Value.Should().Be(consolidatedToReturn);
        _entryDaoMock.Verify(dao => dao.ListEntriesByDateAsync(It.IsAny<DateOnly>(), It.IsAny<long>()), Times.Never);
        _dailyConsolidatedDaoMock.Verify(dao => dao.GetConsolidatedUpdatedAsync(It.Is<DateOnly>(d => d == consultationDate)), Times.Once);
    }

    [Fact]
    public async Task DailyConsolidatedReportExtendedHandlerAsync_InvalidDate_ReturnsErrorMessage()
    {
        var consultationDate = "invalid_date";

        var result = await DailyConsolidatedReportEndpoints.DailyConsolidatedReportExtendedGetHandlerAsync
            (_dailyConsolidatedDaoMock.Object, _entryDaoMock.Object, consultationDate);

        var resultType = result.Should().BeOfType<Results<Ok<DailyConsolidatedReportExtendedVM>, ProblemHttpResult>>().Subject;
        var problemHttpResult = resultType.Result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemHttpResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemHttpResult.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        GetProblemDetailsErrors(problemHttpResult.ProblemDetails)
            .Should().BeEquivalentTo(["The date format is invalid. Use 'yyyy-MM-dd'."]);

        _entryDaoMock.Verify(dao => dao.ListEntriesByDateAsync(It.IsAny<DateOnly>(), It.IsAny<long>()), Times.Never);
        _dailyConsolidatedDaoMock.Verify(dao => dao.GetConsolidatedUpdatedAsync(It.IsAny<DateOnly>()), Times.Never);
    }
}
