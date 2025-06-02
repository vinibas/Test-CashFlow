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

    public DailyConsolidatedReportEndpointsTests()
    {
        _dailyConsolidatedDaoMock = new Mock<IDailyConsolidatedDao>();
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
        _dailyConsolidatedDaoMock.Setup(dao =>
            dao.GetConsolidatedUpdatedAsync(It.IsAny<DateOnly>()));

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
}
