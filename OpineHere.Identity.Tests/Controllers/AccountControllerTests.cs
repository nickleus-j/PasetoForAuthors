using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OpineHere.Data;
using Xunit;
using OpineHere.Identity.Controllers;
using OpineHere.Identity.Dto;
using OpineHere.Identity.Service;
using OpineHere.Identity.Token;

namespace OpineHere.Identity.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IDataUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<IPasetoTokenMakerService> _mockPasetoService;
    private readonly Mock<ITokenRevocationStore> _mockRevocationStore;
    private readonly Mock<ILogger<PasetoController>> _mockLogger;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
        _mockTokenService = new Mock<ITokenService>();
        _mockUnitOfWork = new Mock<IDataUnitOfWork>();
        _mockEmailSender = new Mock<IEmailSender>();
        _mockPasetoService = new Mock<IPasetoTokenMakerService>();
        _mockRevocationStore = new Mock<ITokenRevocationStore>();
        _mockLogger = new Mock<ILogger<PasetoController>>();

        _controller = new AccountController(
            _mockUserManager.Object,
            _mockTokenService.Object,
            _mockUnitOfWork.Object,
            _mockEmailSender.Object,
            _mockPasetoService.Object,
            _mockRevocationStore.Object,
            _mockLogger.Object);
    }

    #region RequestMagicLink Tests

    [Fact]
    public async Task RequestMagicLink_WithValidEmail_ReturnsOkResult()
    {
        // Arrange
        string email = "test@example.com";
        string baseUrl = "https://example.com/login";
        string generatedToken = "test_token_123";

        _mockPasetoService
            .Setup(s => s.GenerateMagicLinkToken(email, email))
            .Returns(generatedToken);

        _mockEmailSender
            .Setup(s => s.SendMagicLinkAsync(email, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RequestMagicLink(email, baseUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockPasetoService.Verify(s => s.GenerateMagicLinkToken(email, email), Times.Once);
        _mockEmailSender.Verify(s => s.SendMagicLinkAsync(email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RequestMagicLink_GeneratesMagicLinkWithCorrectFormat()
    {
        // Arrange
        string email = "test@example.com";
        string baseUrl = "https://example.com/login";
        string generatedToken = "v2.public.test_token";

        _mockPasetoService
            .Setup(s => s.GenerateMagicLinkToken(email, email))
            .Returns(generatedToken);

        _mockEmailSender
            .Setup(s => s.SendMagicLinkAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RequestMagicLink(email, baseUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Capture the actual magic link sent
        _mockEmailSender.Verify(
            s => s.SendMagicLinkAsync(
                email,
                It.Is<string>(link =>
                    link.StartsWith(baseUrl) &&
                    link.Contains("token=") &&
                    link.Contains(Uri.EscapeDataString(generatedToken)))),
            Times.Once);
    }

    [Fact]
    public async Task RequestMagicLink_WithEmptyEmail_SendsEmailWithGeneratedToken()
    {
        // Arrange
        string email = "";
        string baseUrl = "https://example.com/login";
        string generatedToken = "empty_email_token";

        _mockPasetoService
            .Setup(s => s.GenerateMagicLinkToken(email, email))
            .Returns(generatedToken);

        _mockEmailSender
            .Setup(s => s.SendMagicLinkAsync(email, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RequestMagicLink(email, baseUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockEmailSender.Verify(s => s.SendMagicLinkAsync(email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RequestMagicLink_ReturnsSuccessMessage()
    {
        // Arrange
        string email = "test@example.com";
        string baseUrl = "https://example.com/login";

        _mockPasetoService
            .Setup(s => s.GenerateMagicLinkToken(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("token");

        _mockEmailSender
            .Setup(s => s.SendMagicLinkAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RequestMagicLink(email, baseUrl);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        var messageProperty = response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("If the email is registered, a magic login link has been sent.",
            messageProperty.GetValue(response));
    }

    #endregion

    #region VerifyMagicLink Tests

    [Fact]
    public async Task VerifyMagicLink_WithValidToken_ReturnsOkResult()
    {
        // Arrange
        string token = "valid_token_123";
        var tokenResult = new PasetoTokenValidationResult
        (
            false,
            "user_123",
            "emailUp@ee.co",
            token,
            "Token has expired"
        );
        _mockPasetoService
            .Setup(s => s.ValidateMagicLinkToken(token))
            .Returns(tokenResult);

        _mockRevocationStore
            .Setup(s => s.IsTokenUsedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockRevocationStore
            .Setup(s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.VerifyMagicLink(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockRevocationStore.Verify(
            s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyMagicLink_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        string token = "invalid_token";
        var tokenResult = new PasetoTokenValidationResult
        (
            false,
              "user_123",
            "emailUp@ee.co",
            "invalid_token_123",
             "Token has expired"
        );

        _mockPasetoService
            .Setup(s => s.ValidateMagicLinkToken(token)).Returns(tokenResult);

        // Act
        var result = await _controller.VerifyMagicLink(token);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        var errorProperty = badRequestResult.Value.GetType().GetProperty("error");
        Assert.NotNull(errorProperty);
        Assert.Equal("Token has expired", errorProperty.GetValue(badRequestResult.Value));
    }

    [Fact]
    public async Task VerifyMagicLink_WithAlreadyUsedToken_ReturnsBadRequest()
    {
        // Arrange
        string token = "used_token";
        
        var tokenResult = new PasetoTokenValidationResult
        (
            true,
            "user_123",
            "emailUp@ee.co",
            token,
            "Token has expired"
        );
        _mockPasetoService
            .Setup(s => s.ValidateMagicLinkToken(token))
            .Returns(tokenResult);

        _mockRevocationStore
            .Setup(s => s.IsTokenUsedAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyMagicLink(token);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        var errorProperty = badRequestResult.Value.GetType().GetProperty("error");
        Assert.NotNull(errorProperty);
        Assert.Equal("This magic link has already been used.", errorProperty.GetValue(badRequestResult.Value));
    }

    [Fact]
    public async Task VerifyMagicLink_WithValidToken_MarkTokenAsUsedWithFifteenMinuteTtl()
    {
        // Arrange
        string token = "valid_token";
        var tokenResult = new PasetoTokenValidationResult
        (
            true,
            "user_456",
            "emailUp@ee.co",
            token,
            "Token has expired"
        );
        _mockPasetoService
            .Setup(s => s.ValidateMagicLinkToken(token))
            .Returns(tokenResult);

        _mockRevocationStore
            .Setup(s => s.IsTokenUsedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockRevocationStore
            .Setup(s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.VerifyMagicLink(token);

        // Assert
        _mockRevocationStore.Verify(
            s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), TimeSpan.FromMinutes(15)),
            Times.Once);
    }

    [Fact]
    public async Task VerifyMagicLink_WithValidToken_ReturnsCorrectData()
    {
        // Arrange
        string token = "valid_token_data_check";
        var tokenResult = new PasetoTokenValidationResult
        (
            true,
            "user_data_123",
            "data@example.com",
            token,
            "Token has expired"
        );
        _mockPasetoService
            .Setup(s => s.ValidateMagicLinkToken(token))
            .Returns(tokenResult);

        _mockRevocationStore
            .Setup(s => s.IsTokenUsedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockRevocationStore
            .Setup(s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.VerifyMagicLink(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);

        var messageProperty = response.GetType().GetProperty("message");
        Assert.Equal("Authentication successful", messageProperty.GetValue(response));

        var authorIdProperty = response.GetType().GetProperty("authorId");
        Assert.Equal("user_data_123", authorIdProperty.GetValue(response));

        var emailProperty = response.GetType().GetProperty("email");
        Assert.Equal("data@example.com", emailProperty.GetValue(response));
    }

    [Fact]
    public async Task VerifyMagicLink_WithInvalidToken_DoesNotCallRevocationStore()
    {
        // Arrange
        string token = "invalid_token";
        var tokenResult = new PasetoTokenValidationResult
        (
            false,
            null,
            null,
            token,
            "Token has expired"
        );
        _mockPasetoService
            .Setup(s => s.ValidateMagicLinkToken(token))
            .Returns(tokenResult);

        // Act
        var result = await _controller.VerifyMagicLink(token);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _mockRevocationStore.Verify(
            s => s.IsTokenUsedAsync(It.IsAny<string>()),
            Times.Never);
        _mockRevocationStore.Verify(
            s => s.MarkTokenAsUsedAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    #endregion
}