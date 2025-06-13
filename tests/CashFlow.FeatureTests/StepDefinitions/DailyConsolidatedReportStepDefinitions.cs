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
    private const string ReportEndpointResumed = "/api/v1/DailyConsolidatedReport/Summary";
    private const string ReportEndpointExtended = "/api/v1/DailyConsolidatedReport/Extended";

    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    private string? _consultationDate;
    private DateOnly consultationDateParsed => DateOnly.Parse(_consultationDate ?? DateOnly.MinValue.ToString());
    private List<Entry> currentEntriesOnDB = [];

    private HttpResponseMessage? _response;
    private DailyConsolidatedResponseData? _dailyConsolidatedReturned;
    private DailyConsolidatedResponseData? _dailyConsolidatedExpected;

    string reportType = "";

    public DailyConsolidatedReportStepDefinitions(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();

        _factory.CleanDatabase();
        currentEntriesOnDB = [];
    }

    [Given(@"I have the following entries in my database:")]
    public void GivenIHaveTheFollowingEntriesInMyDatabase(Table table)
    {
        // Additional logic required for the fields generated or calculated in the database.
        var random = new Random();
        using var scope = _factory.Services.CreateScope();
        var cx = scope.ServiceProvider.GetRequiredService<CashFlowContext>();

        Type entryType = typeof(Entry);
        Type dailyConsolidatedType = typeof(DailyConsolidated);

        var lineNumber = 0;
        foreach (var row in table.Rows)
        {
            var date = DateOnly.Parse(row["date"].Trim('"'));
            var value = decimal.Parse(row["value"]);
            var type = row["type"].Trim('"');

            var dateWithRandomTime = date.ToDateTime(new TimeOnly(random.Next(0, 24), random.Next(0, 60)));

            var entry = new Entry(value, (EntryType)type[0], "Feature test description", dateWithRandomTime);

            entryType.GetProperty(nameof(Entry.LineNumber))!.SetValue(entry, ++lineNumber);

            currentEntriesOnDB.Add(entry);
        }

        var reports = currentEntriesOnDB
            .GroupBy(e => DateOnly.FromDateTime(e.TransactionAtUtc))
            .Select(g =>
            {
                var report = new DailyConsolidated(
                    g.Key,
                    g.Where(e => e.Type == EntryType.Credit).Sum(e => e.Value),
                    g.Where(e => e.Type == EntryType.Debit).Sum(e => e.Value));

                var lastLineNumberCalculated = g.Max(e => e.LineNumber);
                dailyConsolidatedType.GetProperty(nameof(DailyConsolidated.LastLineNumberCalculated))!.SetValue(report, lastLineNumberCalculated);

                return report;
            })
            .ToList();

        cx.Entries.AddRange(currentEntriesOnDB);
        cx.DailyConsolidated.AddRange(reports);
        
        cx.SaveChanges();
    }

    [Given(@"{string} as a consultation date")]
    public void GivenDateAsAConsultationDate(string date)
    {
        _consultationDate = date.Equals("today", StringComparison.InvariantCultureIgnoreCase) ?
            DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") : date;
    }

    [Given("there are no entries for this date")]
    public static void GivenThereAreNoEntriesForThisDate() { }

    [When("I request the consolidated report of the type {string} for the consultation date")]
    public async Task WhenIRequestTheConsolidatedReportOfTheTypeForTheConsultationDate(string type)
    {
        var url = type switch
        {
            "resumed" => $"{ReportEndpointResumed}/{_consultationDate}",
            "extended" => $"{ReportEndpointExtended}/{_consultationDate}",
            _ => throw new InvalidOperationException(),
        };
        reportType = type;

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
        _dailyConsolidatedReturned.NetBalance.Should().Be(_dailyConsolidatedExpected.NetBalance);
        _dailyConsolidatedReturned.IsClosed.Should().Be(_dailyConsolidatedExpected.IsClosed);
        _dailyConsolidatedReturned.Entries.Should().BeEquivalentTo(_dailyConsolidatedExpected.Entries);
    }

    private async Task FillOutDailyConsolidatedReturnedAsync()
    {
        _dailyConsolidatedReturned = await _response!.Content.ReadFromJsonAsync<DailyConsolidatedResponseData>() ??
            throw new InvalidOperationException("Response could not be deserialized as DailyConsolidated");
    }

    private void FillOutDailyConsolidatedExpected(Table table)
    {
        var row = table.Rows[0];
        var date = row["date"].Equals("\"today\"", StringComparison.InvariantCultureIgnoreCase) ?
            DateOnly.FromDateTime(DateTime.UtcNow) :
            DateOnly.Parse(row["date"].Trim('"'));

        var expectedEntries = reportType is "extended" ?
            currentEntriesOnDB
                .Where(e => DateOnly.FromDateTime(e.TransactionAtUtc) == consultationDateParsed)
                .OrderBy(e => e.TransactionAtUtc)
                .Select(e => new EntryResponseData(e.Value, (char)e.Type))
                .ToList() :
                null;
            
        _dailyConsolidatedExpected = new DailyConsolidatedResponseData(
            date,
            expectedEntries,
            decimal.Parse(row["totalCredits"]),
            decimal.Parse(row["totalDebits"]),
            decimal.Parse(row["netBalance"]),
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

    public record DailyConsolidatedResponseData(
        DateOnly Date,
        IEnumerable<EntryResponseData>? Entries,
        decimal TotalCredits,
        decimal TotalDebits,
        decimal NetBalance,
        bool IsClosed);
    public record EntryResponseData(decimal Value, char Type);
}
