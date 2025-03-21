using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BN.PROJECT.IdentityService.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IIdentityRepository> _mockIdentityRepository;
        private readonly Mock<IKeycloakServiceClient> _mockKeycloakServiceClient;
        private readonly Mock<IStrategyServiceClient> _mockStrategyServiceClient;
        private readonly Mock<IAlpacaServiceClient> _mockAlpacaServiceClient;

        private readonly Mock<IEmailService> _mockEmailService;


        private readonly AccountController _controller;

        private readonly string _accessToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJ2a0Mzc011TndrRGhIS1FHVk5nVk13em5KZW9VVkItWmhwZGcxTW9ibjEwIn0.eyJleHAiOjE3MzY0MjIzMTcsImlhdCI6MTczNjQyMjAxNywianRpIjoiYjU3MDg3MzAtMzk0YS00NGY4LWEwYzQtMGFhMTQzNjFhNDY1IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrOjgwODAvcmVhbG1zL2JuLXByb2plY3QiLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiOTFlNjVhYjctZDM4MC00MTQwLTkzNjMtZWRiN2M3N2U0NmU3IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiYm4tcHJvamVjdC1jbGllbnQiLCJzZXNzaW9uX3N0YXRlIjoiMDBkMWNkOTktNDFjYy00OTgxLTg5M2EtYzcyNWZjYTc2ODVmIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyIqIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsImRlZmF1bHQtcm9sZXMtYm4tcHJvamVjdCIsInVtYV9hdXRob3JpemF0aW9uIiwidXNlciJdfSwicmVzb3VyY2VfYWNjZXNzIjp7ImFjY291bnQiOnsicm9sZXMiOlsibWFuYWdlLWFjY291bnQiLCJtYW5hZ2UtYWNjb3VudC1saW5rcyIsInZpZXctcHJvZmlsZSJdfX0sInNjb3BlIjoiZW1haWwgcHJvZmlsZSIsInNpZCI6IjAwZDFjZDk5LTQxY2MtNDk4MS04OTNhLWM3MjVmY2E3Njg1ZiIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwibmFtZSI6IlVscmljaCBIZXJ3aWciLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJkb2dvYmVydCIsImdpdmVuX25hbWUiOiJVbHJpY2giLCJmYW1pbHlfbmFtZSI6IkhlcndpZyIsImVtYWlsIjoidWxpLWhlcndpZ0B0ZXN0LmRlIn0.ZHMAhNeJv7-u5HBxqHVnn6KLDCiUlFnhDoPDResna0ZcII3JvCIMjZQRyq7nMNTIw2NgT4Eof8yK43SrsYv30lKf9UdKUXz30PPkLxNSFhmriJPXUyCPcTTiCt4oiJgXuLmT-cEnkPOr0-9nOTQTDJNNY83wuDJv6RTNd-7uSt741oVDDEQt-ZLKOkpAL8UcMQjI1mlWvX8MVX0iJxtGTuZTolMi6gDUD3YKgLj0BBzH1X6ZG_DD1Ne7YTyoecSejRTOwlBZlRwjraVUVx8drRAqvNI_5hNk1m_lPjaUrK7501GAmOtHyNQnXjds8Z6asfrorztb8f1i8s1Pa77F_w";

        public AccountControllerTests()
        {
            _mockIdentityRepository = new Mock<IIdentityRepository>();
            _mockKeycloakServiceClient = new Mock<IKeycloakServiceClient>();
            _mockStrategyServiceClient = new Mock<IStrategyServiceClient>();
            _mockAlpacaServiceClient = new Mock<IAlpacaServiceClient>();
            _mockEmailService = new Mock<IEmailService>();

            _controller = new AccountController(
                _mockIdentityRepository.Object,
                _mockStrategyServiceClient.Object,
                _mockAlpacaServiceClient.Object,
                _mockKeycloakServiceClient.Object,
                _mockEmailService.Object);
                
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
            var signOutRequest = new SignOutRequest { RefreshToken = _accessToken };
            var signOutResponse = new SignOutResponse { Success = true };
            var claims = new Dictionary<string, string> { { "sub", Guid.NewGuid().ToString() } };

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
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = _accessToken };
            var refreshTokenResponse = new JwtToken { RefreshToken = _accessToken };

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

    }
}
