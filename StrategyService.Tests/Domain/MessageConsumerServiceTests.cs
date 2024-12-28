using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using BN.PROJECT.Core;
using NuGet.Protocol;


namespace BN.PROJECT.StrategyService.Tests
{
    public class MessageConsumerServiceTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IKafkaConsumerService> _kafkaConsumerServiceMock;
        private readonly TestLogger<MessageConsumerService> _testLogger;
        private readonly MessageConsumerService _messageConsumerService;

        public MessageConsumerServiceTests()
        {
            _testLogger = new TestLogger<MessageConsumerService>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            _kafkaConsumerServiceMock = new Mock<IKafkaConsumerService>();

            _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);
            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);


            _messageConsumerService = new MessageConsumerService(_testLogger, _serviceProviderMock.Object);
        }

        [Fact]
        public async Task StartAsync_ShouldStartKafkaConsumer()
        {
            // Arrange
            _serviceProviderMock.Setup(x => x.GetService(typeof(IKafkaConsumerService))).Returns(_kafkaConsumerServiceMock.Object);
            _kafkaConsumerServiceMock.Setup(x => x.Start(It.IsAny<string>()));

            // Act
            await _messageConsumerService.StartAsync(CancellationToken.None);

            // Assert
            _kafkaConsumerServiceMock.Verify(x => x.Start("strategy"), Times.Once);
        }

        [Fact]
        public async Task StartAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            _serviceProviderMock.Setup(x => x.GetService(typeof(IKafkaConsumerService))).Returns(_kafkaConsumerServiceMock.Object);
            var exception = new Exception("Test exception");
            _serviceProviderMock.Setup(x => x.GetService(typeof(IKafkaConsumerService))).Throws(exception);

            // Act
            await _messageConsumerService.StartAsync(CancellationToken.None);

            // Assert
            Assert.Contains(_testLogger.LoggedMessages, log => log.LogLevel == LogLevel.Error && log.Message == "Error in MessageConsumerService" && log.Exception == exception);
        }

        [Fact]
        public void ConsumeMessage_ShouldDeserializeMessage_WhenMessageIsNotNull()
        {
            // Arrange
            var message = new StrategyMessage { Strategy = StrategyEnum.SMA, MessageType = MessageTypeEnum.StartTest };
            var messageJson = message.ToJson();

            var strategyServiceMock = new Mock<IStrategyService>();
            strategyServiceMock.Setup(x => x.CanHandle(message.Strategy)).Returns(true);

            var strategyServices = new List<IStrategyService> { strategyServiceMock.Object };

            _serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IStrategyService>)))
                                .Returns(strategyServices);

            // Act
            _messageConsumerService.ConsumeMessage(messageJson);

            // Assert
            strategyServiceMock.Verify(x => x.StartTest(It.Is<StrategyMessage>(m => m.Strategy == message.Strategy && m.MessageType == message.MessageType)), Times.Once);
        }


        [Fact]
        public async Task StopAsync_ShouldReturnCompletedTask()
        {
            // Act
            var result = _messageConsumerService.StopAsync(CancellationToken.None);

            // Assert
            Assert.Equal(Task.CompletedTask, result);
        }
    }



}
