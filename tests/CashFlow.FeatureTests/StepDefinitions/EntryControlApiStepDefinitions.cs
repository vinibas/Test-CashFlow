using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace CashFlow.FeatureTests.StepDefinitions;

[Binding]
public class EntryControlApiStepDefinitions : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string EntryControlEndpoint = "/api/v1/EntryControl";
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    private object? _entryRequest;
    private HttpResponseMessage? _response;

    public EntryControlApiStepDefinitions(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Given("I have a valid entry with value {decimal} and type {string}")]
    public void GivenIHaveAValidEntryWithValueAndType(decimal value, char type)
    {
        _entryRequest = new { Value = value, Type = type };
    }

    [When("I send a POST request to Entry Control endpoint with this entry")]
    public async Task WhenISendAPostRequestToEntryControlWithThisEntry()
    {
        _response = await _httpClient.PostAsJsonAsync(EntryControlEndpoint, _entryRequest);
    }

    [Then("the response status code should be 201")]
    public void ThenTheResponseStatusCodeShouldBe201()
    {
        Assert.Equal(HttpStatusCode.Created, _response?.StatusCode);
    }

    [Then("the entry should be created successfully")]
    public void ThenTheEntryShouldBeCreatedSuccessfully()
    {
        throw new PendingStepException();
        // using (var scope = _factory.Services.CreateScope())
        // {
        //     var db = scope.ServiceProvider.GetService<DbContext>();

        // }

    }
}
