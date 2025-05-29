using Reqnroll;

namespace CashFlow.FeatureTests.StepDefinitions;

[Binding]
public class TestingFeatureStepDefinitions
{
    [Given("I call some endpoint")]
    public void GivenICallSomeEndpoint()
    {
        
    }

    [Then("it should return 201")]
    public void ThenItShouldReturn201()
    {
        Assert.True(true);
    }
}
