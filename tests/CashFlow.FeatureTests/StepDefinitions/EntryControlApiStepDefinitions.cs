using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using CashFlow.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace CashFlow.FeatureTests.StepDefinitions;

[Binding]
public class EntryControlApiStepDefinitions : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string EntryControlEndpoint = "/api/v1/EntryControl";
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    private EntryRequest? _entryRequest;
    private HttpResponseMessage? _response;

    public EntryControlApiStepDefinitions(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Given("I have a entry with value {decimal} and type {string}")]
    public void GivenIHaveAValidEntryWithValueAndType(decimal value, char type)
    {
        _entryRequest = new (value, type );
    }

    [When("I send a POST request to Entry Control endpoint with this entry")]
    public async Task WhenISendAPostRequestToEntryControlWithThisEntry()
    {
        _response = await _httpClient.PostAsJsonAsync(EntryControlEndpoint, _entryRequest);
    }

    [Then("the response status code of the Entry control endpoint should be {int}")]
    public void ThenTheResponseStatusCodeOfTheEntryControlEndpointShouldBe(int statusCode)
    {
        _response!.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then("the response should be an ErrorDetails with the messages {string}")]
    public async Task ThenTheResponseStatusCodeShouldBe(string messages)
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
        errors.Should().BeEquivalentTo(messages.Split(','));

        problemDetails!.Detail.Should().Be(messages);
    }

    [Then("the entry should be created successfully")]
    public void ThenTheEntryShouldBeCreatedSuccessfully()
    {
        using var scope = _factory.Services.CreateScope();
        var cx = scope.ServiceProvider.GetRequiredService<CashFlowContext>();

        var allEntries = cx.Entries.ToList();

        allEntries.Should().ContainSingle(e => e.Value == _entryRequest!.Value && (char)e.Type == _entryRequest.Type);
    }

    [Then("the entry should not be created")]
    public void ThenTheEntryShouldNotBeCreated()
    {
        using var scope = _factory.Services.CreateScope();
        var cx = scope.ServiceProvider.GetRequiredService<CashFlowContext>();

        cx.Entries.ToList().Should().BeEmpty();
    }

    sealed record EntryRequest(decimal Value, char Type);
}
