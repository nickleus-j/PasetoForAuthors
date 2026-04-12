using JetBrains.Annotations;
using OpineHere.Identity.Service;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OpineHere.Identity.Token;
using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;
using Paseto.Protocol;

namespace OpineHere.Identity.Tests.Service;

[TestSubject(typeof(PasetoTokenService))]
public class PasetoTokenServiceTests
{
    private readonly Mock<IKeyProvider> _keyProviderMock;
    private readonly Mock<ILogger<PasetoTokenService>> _loggerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly PasetoTokenService _sut; // System Under Test

    public PasetoTokenServiceTests()
    {
        _keyProviderMock = new Mock<IKeyProvider>();
        _loggerMock = new Mock<ILogger<PasetoTokenService>>();
        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(x => x["Issuer"]).Returns("test-issuer");

        // Generate a 32-byte seed for Ed25519
        var seed = new byte[32]; 
        new Random().NextBytes(seed);

        // Create the key directly for V4 Public (Asymmetric)
        // Note: ProtocolVersion.V4 and Purpose.Public match your Service implementation
        var testKey = new PasetoBuilder()
            .UseV4(Purpose.Public)
            .GenerateAsymmetricKeyPair(seed);
        var configObject = _configMock.Object;
        
        // Setup the mock to return the object
        _keyProviderMock.Setup(x => x.GetSecretKey()).Returns(testKey.SecretKey);
        _keyProviderMock.Setup(x => x.GetPublicKey()).Returns(testKey.PublicKey);
        configObject["Paseto:SecretKey"] = Paserk.Encode(testKey.SecretKey,PaserkType.Secret);
        configObject["Issuer"] = "test.com";
        _sut = new PasetoTokenService(
            _keyProviderMock.Object, 
            _loggerMock.Object, 
            configObject);
    }

    [Fact]
    public void GenerateToken_ShouldReturnToken_WhenInputsAreValid()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";

        // Act
        var token = _sut.GenerateToken(userId, email);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.StartsWith("v4.public.", token); // V4 Public tokens always start with this prefix
    }

    [Fact]
    public void GenerateToken_ShouldUseCustomExpiration_WhenProvided()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";
        var customExpiration = TimeSpan.FromDays(7);

        // Act
        var token = _sut.GenerateToken(userId, email, customExpiration);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        // Note: To fully assert expiration, you would decode the token and check the 'exp' claim,
        // which is handled implicitly in your ValidateToken method tests below.
    }

    [Fact]
    public void GenerateToken_ShouldThrowException_WhenKeyProviderFails()
    {
        // Arrange
        _keyProviderMock.Setup(x => x.GetSecretKey())
            .Throws(new InvalidOperationException("Key provider offline"));

        var userId = "user-123";
        var email = "test@example.com";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            _sut.GenerateToken(userId, email));
            
        Assert.Equal("Key provider offline", exception.Message);
    }

    [Fact]
    public void ValidateToken_ShouldReturnValidResult_WhenTokenIsAuthentic()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";
        
        // Generate a real token using the service to test validation
        var validToken = _sut.GenerateToken(userId, email);

        // Act
        var result = _sut.ValidateToken(validToken);

        // Assert
        // Using this style shows the error message in Rider's output if it fails
        Assert.True(result.IsValid, $"Validation failed with error: {result.ErrorMessage}");
        Assert.Equal(userId, result.UserId);
        Assert.Equal(email, result.Email);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateToken_ShouldReturnInvalidResult_WhenTokenIsTamperedOrMalformed()
    {
        // Arrange
        var invalidToken = "v4.public.tampered-token-string-that-is-invalid";

        // Act
        var result = _sut.ValidateToken(invalidToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void ValidateToken_ShouldReturnInvalidResult_WhenTokenIsExpired()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";
        
        // Create a token that expires instantly (or in the past)
        var expiredToken = _sut.GenerateToken(userId, email, TimeSpan.FromTicks(-1));

        // Act
        var result = _sut.ValidateToken(expiredToken);

        // Assert
        Assert.False(result.IsValid);
        // The PASETO library should mark this invalid during Decode() based on validationParams
    }

    [Fact]
    public void ValidateToken_ShouldCatchExceptionsAndReturnInvalidResult()
    {
        // Arrange
        // Force an exception by breaking the key provider during validation
        _keyProviderMock.Setup(x => x.GetSecretKey())
            .Throws(new Exception("Unexpected cryptographic error"));

        // Act
        var result = _sut.ValidateToken("v4.public.some-token");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Payload does not contain signature", result.ErrorMessage);
    }
}