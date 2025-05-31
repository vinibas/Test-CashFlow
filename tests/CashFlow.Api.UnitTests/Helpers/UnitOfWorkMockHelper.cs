using CashFlow.Api.Data;
using Moq;

namespace CashFlow.Api.UnitTests.Helpers;

public class UnitOfWorkMockHelper
{
    public Mock<IUnitOfWork> UowMock { get; } = new Mock<IUnitOfWork>();

    public void SetupDbContextMockAsync() => UowMock.Setup(uow => uow.CommitAsync());
    public void VerifyIfCommitAsyncWasCalledOnce()
        => UowMock.Verify(uow => uow.CommitAsync(), Times.Once);
    public void VerifyIfCommitAsyncWasCalledNever()
        => UowMock.Verify(uow => uow.CommitAsync(), Times.Never);
}
