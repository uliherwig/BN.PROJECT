using BN.PROJECT.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.StrategyService.Tests;
public class StrategyServiceStoreTests
{ 
    
    private readonly Mock<ILogger<StrategyServiceStore>> _mockLogger;

    private readonly Mock<IServiceProvider> _serviceProviderMock;  
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;

    private StrategyServiceStore _store;

    public StrategyServiceStoreTests()
    {
        _mockLogger = new Mock<ILogger<StrategyServiceStore>>();

        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
         _store = new StrategyServiceStore(_serviceProviderMock.Object);

    }

    //[Fact]
    //public void GetOrCreate_Should_Return_Same_Instance_For_Same_StrategyId()
    //{
    //    // Arrange
    //    var optimizerMock = new Mock<IOptimizerService>();
    //    _serviceProviderMock.Setup(x => x.GetService(typeof(IOptimizerService))).Returns(optimizerMock.Object);
    //    var strategyId = Guid.NewGuid();

    //    // Act
    //    var instance1 = _store.GetOrCreateOptimizer(strategyId);
    //    var instance2 = _store.GetOrCreateOptimizer(strategyId);

    //    // Assert
    //    Assert.Same(instance1, instance2); // same reference
    //}

    //[Fact]
    //public void GetOrCreate_Should_Return_Different_Instance_For_Different_StrategyId()
    //{
    //    // Arrange
    //    var optimizerMock1 = new Mock<IOptimizerService>();
    //    var optimizerMock2 = new Mock<IOptimizerService>();

    //    // Act
    //    _serviceProviderMock.Setup(x => x.GetService(typeof(IOptimizerService))).Returns(optimizerMock1.Object);
    //    var strategy1 = _store.GetOrCreateOptimizer(Guid.NewGuid());
    //    _serviceProviderMock.Setup(x => x.GetService(typeof(IOptimizerService))).Returns(optimizerMock2.Object);
    //    var strategy2 = _store.GetOrCreateOptimizer(Guid.NewGuid());

    //    // Assert
    //    Assert.NotSame(strategy1, strategy2);
    //}

    //[Fact]
    //public void GetOrCreate_Should_Be_ThreadSafe()
    //{
    //    // Arrange
    //    var optimizerMock = new Mock<IOptimizerService>();
    //    _serviceProviderMock.Setup(x => x.GetService(typeof(IOptimizerService))).Returns(optimizerMock.Object);

    //    var strategyId = Guid.NewGuid();
    //    IOptimizerService[] results = new IOptimizerService[100];

    //    // Act
    //    Parallel.For(0, 100, i =>
    //    {
    //        results[i] = _store.GetOrCreateOptimizer(strategyId);
    //    });

    //    // Assert
    //    for (int i = 1; i < results.Length; i++)
    //    {
    //        Assert.Same(results[0], results[i]); // all must point to the same instance
    //    }
    //} 


}

