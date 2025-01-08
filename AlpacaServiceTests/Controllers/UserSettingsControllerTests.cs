using BN.PROJECT.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class UserSettingsControllerTests
    {
        private readonly Mock<IAlpacaRepository> _mockAlpacaRepository;
        private readonly Mock<ILogger<UserSettingsController>> _mockLogger;
        private readonly UserSettingsController _controller;
        private readonly UserSettings _userSettings = new UserSettings { UserId = "testUser", AlpacaKey = "123", AlpacaSecret = "456" };

        public UserSettingsControllerTests()
        {
            _mockAlpacaRepository = new Mock<IAlpacaRepository>();
            _mockLogger = new Mock<ILogger<UserSettingsController>>();
            _controller = new UserSettingsController(_mockAlpacaRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task AddUserSettingsAsync_ShouldReturnBadRequest_WhenUserSettingsIsNull()
        {
            // Act
            var result = await _controller.AddUserSettingsAsync(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("UserSettings cannot be null", badRequestResult.Value);
        }

        [Fact]
        public async Task AddUserSettingsAsync_ShouldReturnOk_WhenUserSettingsIsValid()
        {
            // Arrange
            _mockAlpacaRepository.Setup(repo => repo.AddUserSettingsAsync(_userSettings))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddUserSettingsAsync(_userSettings);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task GetUserSettingsAsync_ShouldReturnNotFound_WhenUserSettingsNotFound()
        {
            // Arrange
            var userId = "testUser";

            _mockAlpacaRepository.Setup(repo => repo.GetUserSettingsAsync(userId))
                .ReturnsAsync((UserSettings)null);

            // Act
            var result = await _controller.GetUserSettingsAsync(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(ErrorCode.NotFound, notFoundResult.Value);
        }

        [Fact]
        public async Task GetUserSettingsAsync_ShouldReturnOk_WhenUserSettingsFound()
        {
            // Arrange
            var userId = "testUser";

            _mockAlpacaRepository.Setup(repo => repo.GetUserSettingsAsync(userId))
                .ReturnsAsync(_userSettings);

            // Act
            var result = await _controller.GetUserSettingsAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(_userSettings, okResult.Value);
        }

        [Fact]
        public async Task UpdateUserSettingsAsync_ShouldReturnBadRequest_WhenUserSettingsIsNull()
        {
            // Act
            var result = await _controller.UpdateUserSettingsAsync(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("UserSettings cannot be null", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserSettingsAsync_ShouldReturnOk_WhenUserSettingsIsValid()
        {
            // Arrange
            _mockAlpacaRepository.Setup(repo => repo.UpdateUserSettingsAsync(_userSettings))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateUserSettingsAsync(_userSettings);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task DeleteUserSettingsAsync_ShouldReturnBadRequest_WhenUserSettingsIsNull()
        {
            // Act
            var result = await _controller.DeleteUserSettingsAsync(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("UserSettings cannot be null", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteUserSettingsAsync_ShouldReturnNoContent_WhenUserSettingsIsValid()
        {
            // Arrange
            _mockAlpacaRepository.Setup(repo => repo.DeleteUserSettingsAsync(_userSettings))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteUserSettingsAsync(_userSettings);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}