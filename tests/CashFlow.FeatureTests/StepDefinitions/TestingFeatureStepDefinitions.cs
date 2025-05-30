using System.Net;
using System.Net.Http.Json;
using Reqnroll;

namespace CashFlow.FeatureTests.StepDefinitions;

[Binding]
public class TestingFeatureStepDefinitions : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    private HttpResponseMessage? response;

    public TestingFeatureStepDefinitions(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Given("I call some endpoint")]
    public async Task GivenICallSomeEndpoint()
    {
        response = await _httpClient.PostAsync("/api/v1/EntryControl", null);
    }

    [Then("it should return 201")]
    public void ThenItShouldReturn201()
    {
        Assert.Equal(HttpStatusCode.Created, response?.StatusCode);
    }
}
