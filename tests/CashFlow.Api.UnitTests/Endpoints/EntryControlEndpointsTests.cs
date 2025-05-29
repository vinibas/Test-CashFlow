using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using CashFlow.Api.Endpoints;

namespace CashFlow.Api.UnitTests.Endpoints;

public class EntryControlEndpointsTests
{
    [Fact]
    public void EntryControlPostHandler_WhenToCall_ReturnsSuccess()
    {
        var result = EntryControlEndpoints.EntryControlPostHandler();
        Assert.IsType<Created>(result);
    }
}