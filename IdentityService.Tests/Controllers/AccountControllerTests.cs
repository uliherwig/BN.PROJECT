using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.IdentityService.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly Mock<IIdentityRepository> _mockIdentityRepository;
        private readonly Mock<IKeycloakServiceClient> _mockKeycloakServiceClient;
        private readonly Mock<IJwtTokenDecoder> _mockJwtTokenDecoder;

        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AccountController>>();
            _mockIdentityRepository = new Mock<IIdentityRepository>();
            _mockKeycloakServiceClient = new Mock<IKeycloakServiceClient>();
            _mockJwtTokenDecoder = new Mock<IJwtTokenDecoder>();

            _controller = new AccountController(
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockIdentityRepository.Object,
                _mockKeycloakServiceClient.Object,
                _mockJwtTokenDecoder.Object);
        }

        [Fact]
        public async Task SignIn_ShouldReturnOk_WhenSignInIsSuccessful()
        {
            // Arrange
            var signInRequest = new SignInRequest { Username = "testUser", Password = "testPassword" };
            var signInResponse = new SignInResponse { Success = true };

            _mockKeycloakServiceClient.Setup(client => client.SignIn(signInRequest))
                .ReturnsAsync(signInResponse);

            _mockIdentityRepository.Setup(repo => repo.GetUserByNameAsync(signInRequest.Username))
                .ReturnsAsync(new User { UserId = Guid.NewGuid(), Username = signInRequest.Username });

            // Act
            var result = await _controller.SignIn(signInRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(signInResponse, okResult.Value);
        }

        [Fact]
        public async Task Logout_ShouldReturnOk_WhenSignOutIsSuccessful()
        {
            // Arrange
            var signOutRequest = new SignOutRequest { RefreshToken = "testRereshToken" };
            var signOutResponse = new SignOutResponse { Success = true };
            var claims = new Dictionary<string, string> { { "sub", Guid.NewGuid().ToString() } };

            _mockJwtTokenDecoder.Setup(decoder => decoder.DecodeJwtToken(signOutRequest.RefreshToken))
                .Returns(claims);
            _mockKeycloakServiceClient.Setup(client => client.SignOut(signOutRequest))
                      .ReturnsAsync(signOutResponse);
            _mockIdentityRepository.Setup(repo => repo.GetSessionByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Session { SessionId = Guid.NewGuid(), UserId = Guid.NewGuid() });

            // Act
            var result = await _controller.Logout(signOutRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(signOutResponse, okResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnOk_WhenRefreshTokenIsSuccessful()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "testRefreshToken" };
            var refreshTokenResponse = new JwtToken { RefreshToken = "newRefreshToken" };

            _mockJwtTokenDecoder.Setup(decoder => decoder.DecodeJwtToken(refreshTokenResponse.RefreshToken))
                .Returns(new Dictionary<string, string> { { "sub", Guid.NewGuid().ToString() } });

            _mockKeycloakServiceClient.Setup(client => client.RefreshToken(refreshTokenRequest))
                .ReturnsAsync(refreshTokenResponse);

            _mockIdentityRepository.Setup(repo => repo.GetSessionByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Session { SessionId = Guid.NewGuid(), UserId = Guid.NewGuid() });

            // Act
            var result = await _controller.RefreshToken(refreshTokenRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(refreshTokenResponse, okResult.Value);
        }

        [Fact]
        public async Task SignUp_ShouldReturnOk_WhenSignUpIsSuccessful()
        {
            // Arrange
            var signUpRequest = new SignUpRequest
            {
                Username = "testUser",
                Password = "testPassword",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            var signUpResponse = new SignUpResponse { Success = true, UserId = Guid.NewGuid().ToString() };

            _mockKeycloakServiceClient.Setup(client => client.SignUp(signUpRequest))
                .ReturnsAsync(signUpResponse);

            // Act
            var result = await _controller.SignUp(signUpRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(signUpResponse, okResult.Value);
        }

        [Fact]
        public void TestAuthorization_ShouldReturnOk_WhenUserIsAdmin()
        {
            // Act
            var result = _controller.TestAuthorization();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }
    }
}