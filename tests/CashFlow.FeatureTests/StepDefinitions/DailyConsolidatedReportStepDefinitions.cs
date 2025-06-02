using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using CashFlow.Api.Data;
using CashFlow.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace CashFlow.FeatureTests.StepDefinitions;

[Binding]
public class DailyConsolidatedReportStepDefinitions : IClassFixture<TestWebApplicationFactory<Program>>, IDisposable
{
    private const string ReportEndpoint = "/api/v1/DailyConsolidatedReport";
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private string? _consultationDate;
    private DateOnly consultationDateParsed => DateOnly.Parse(_consultationDate ?? DateOnly.MinValue.ToString());
    private HttpResponseMessage? _response;

    private DailyConsolidated? _dailyConsolidatedReturned;
    private DailyConsolidatedExpected? _dailyConsolidatedExpected;

    public DailyConsolidatedReportStepDefinitions(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();

        CleanDatabase();
    }

    private void CleanDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var cx = scope.ServiceProvider.GetRequiredService<CashFlowContext>();
        cx.DailyConsolidated.RemoveRange(cx.DailyConsolidated);
        cx.SaveChanges();
    }

    [Given(@"{string} as a consultation date")]
    public void GivenDateAsAConsultationDate(string date)
    {
        _consultationDate = date.Equals("today", StringComparison.InvariantCultureIgnoreCase) ?
            DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") : date;
    }

    [Given(@"there are TotalCredits {decimal} and TotalDebits {decimal} values for this date")]
    public void GivenThereAreTotalCreditsAndTotalDebitsValuesForAConsultationDate(decimal totalCredits, decimal totalDebits)
    {
        using var scope = _factory.Services.CreateScope();
        var cx = scope.ServiceProvider.GetRequiredService<CashFlowContext>();
        var entries = GenerateMassOfData(consultationDateParsed, totalCredits, totalDebits);
        cx.DailyConsolidated.AddRange(entries);
        cx.SaveChanges();
    }

    private static DailyConsolidated[] GenerateMassOfData(DateOnly date, decimal totalCredits, decimal totalDebits)
        =>
        [
            new (date.AddDays(-1), 100.00m, 50.00m),
            new (date, totalCredits, totalDebits),
            new (date.AddDays(1), 300.00m, 150.00m),
        ];

    [Given("there are no entries for this date")]
    public static void GivenThereAreNoEntriesForThisDate() { }

    [When("I request the consolidated report for the consultation date")]
    public async Task WhenIRequestTheConsolidatedReportForTheConsultationDate()
    {
        var url = $"{ReportEndpoint}/{_consultationDate}";
        _response = await _httpClient.GetAsync(url);
    }

    [Then("the response status code of the Consolidated endpoint should be {int}")]
    public void ThenTheResponseStatusCodeOfTheConsolidatedEndpointShouldBe(int statusCode)
    {
        _response!.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then("the report should contain:")]
    public async Task ThenTheReportShouldContain(Table table)
    {
        await FillOutDailyConsolidatedReturnedAsync();
        FillOutDailyConsolidatedExpected(table);

        if (_dailyConsolidatedExpected is null)
            throw new InvalidOperationException("DailyConsolidatedExpected is not set. Please check the table data.");

        _dailyConsolidatedReturned.Should().NotBeNull("DailyConsolidated should not be null");
        _dailyConsolidatedReturned.Date.Should().Be(_dailyConsolidatedExpected.Date);
        _dailyConsolidatedReturned.TotalCredits.Should().Be(_dailyConsolidatedExpected.TotalCredits);
        _dailyConsolidatedReturned.TotalDebits.Should().Be(_dailyConsolidatedExpected.TotalDebits);
        _dailyConsolidatedReturned.IsClosed.Should().Be(_dailyConsolidatedExpected.IsClosed);
    }

    private async Task FillOutDailyConsolidatedReturnedAsync()
    {
        _dailyConsolidatedReturned = await _response!.Content.ReadFromJsonAsync<DailyConsolidated>() ??
            throw new InvalidOperationException("Response could not be deserialized as DailyConsolidated");
    }

    private void FillOutDailyConsolidatedExpected(Table table)
    {
        var row = table.Rows[0];
        var date = row["date"].Equals("\"today\"", StringComparison.InvariantCultureIgnoreCase) ?
            DateOnly.FromDateTime(DateTime.UtcNow) :
            DateOnly.Parse(row["date"].Trim('"'));

        _dailyConsolidatedExpected = new DailyConsolidatedExpected(
            date,
            decimal.Parse(row["totalCredits"]),
            decimal.Parse(row["totalDebits"]),
            bool.Parse(row["isClosed"])
        );
    }

    [Then("the NetBalance value should be the result of credits-debits")]
    public void ThenTheNetBalanceValueShouldBeTheResultOfCreditsMinusDebits()
    {
        if (_dailyConsolidatedExpected is null)
            throw new InvalidOperationException("DailyConsolidatedExpected is not set. Please check the table data.");

        var totalExpected = _dailyConsolidatedExpected.TotalCredits - _dailyConsolidatedExpected.TotalDebits;

        _dailyConsolidatedReturned.Should().NotBeNull("DailyConsolidated should not be null");
        _dailyConsolidatedReturned.NetBalance.Should().Be(totalExpected);
    }

    [Then("the response should contain an error message {string}")]
    public async Task ThenTheResponseShouldContainAnErrorMessage(string message)
    {
        var mediaType = _response!.Content.Headers.ContentType?.MediaType;
        mediaType.Should().Be("application/problem+json");

        var problemDetails = await _response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull("Response could not be deserialized as ProblemDetails");

        var isSuccessJE = (JsonElement?)problemDetails.Extensions["isSuccess"];
        isSuccessJE.Should().NotBeNull();
        isSuccessJE.Value.GetBoolean().Should().BeFalse();

        var errorsJE = (JsonElement?)problemDetails.Extensions["errors"];
        errorsJE.Should().NotBeNull();
        var errors = errorsJE.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
        errors.Should().BeEquivalentTo([message]);

        problemDetails!.Detail.Should().Be(message);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _factory.Dispose();
    }

    public record DailyConsolidatedExpected(DateOnly Date, decimal TotalCredits, decimal TotalDebits, bool IsClosed);
}
