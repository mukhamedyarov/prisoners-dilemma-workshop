using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrisonersDilemma.Api.Authorization;
using PrisonersDilemma.Api.Configuration;
using PrisonersDilemma.Api.Controllers;
using PrisonersDilemma.Api.Data;
using PrisonersDilemma.Api.Exceptions;
using PrisonersDilemma.Api.Models;
using PrisonersDilemma.Api.Services;

namespace PrisonersDilemma.Api.Tests;

public class GameServiceTests : IDisposable
{
    private readonly GameDbContext _context;
    private readonly GameService _gameService;
    
    public GameServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new GameDbContext(options);
        _gameService = new GameService(_context);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    [Fact]
    public async Task CreateGame_ShouldReturnNewGameSession()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        Assert.NotEqual(Guid.Empty, session.SessionId);
        Assert.Equal("Alice", session.Player1.PlayerName);
        Assert.Equal("Bob", session.Player2.PlayerName);
    }
    
    [Fact]
    public async Task GetGame_WithValidId_ShouldReturnSession()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        var retrievedSession = await _gameService.GetGameAsync(session.SessionId);
        
        Assert.NotNull(retrievedSession);
        Assert.Equal(session.SessionId, retrievedSession.SessionId);
    }
    
    [Fact]
    public async Task GetGame_WithInvalidId_ShouldReturnNull()
    {
        var retrievedSession = await _gameService.GetGameAsync(Guid.NewGuid());
        
        Assert.Null(retrievedSession);
    }
    
    [Fact]
    public async Task SetPlayerChoice_WithFirstPlayer_ShouldStorePendingChoice()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        var result = await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
        
        Assert.Null(result);
        
        var updatedSession = await _gameService.GetGameAsync(session.SessionId);
        Assert.NotNull(updatedSession!.PendingRound);
        Assert.Equal(1, updatedSession.PendingRound.RoundNumber);
        Assert.False(updatedSession.PendingRound.IsComplete);
    }
    
    [Fact]
    public async Task SetPlayerChoice_WithBothPlayers_ShouldCompleteRound()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
        var result = await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player2.PlayerId, Choice.Defect);
        
        Assert.NotNull(result);
        Assert.Equal(0, result.Player1Score);
        Assert.Equal(5, result.Player2Score);
        
        var updatedSession = await _gameService.GetGameAsync(session.SessionId);
        Assert.Single(updatedSession!.Results);
        Assert.Null(updatedSession.PendingRound);
    }
    
    [Fact]
    public async Task SetPlayerChoice_WithInvalidPlayerId_ShouldThrowException()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        await Assert.ThrowsAsync<PlayerValidationException>(() => 
            _gameService.SetPlayerChoiceAsync(session.SessionId, Guid.NewGuid(), Choice.Cooperate));
    }
    
    [Fact]
    public async Task SetPlayerChoice_WithInvalidSession_ShouldThrowException()
    {
        await Assert.ThrowsAsync<GameNotFoundException>(() => 
            _gameService.SetPlayerChoiceAsync(Guid.NewGuid(), Guid.NewGuid(), Choice.Cooperate));
    }
    
    [Fact]
    public async Task GetGameStatus_ShouldReturnCorrectStatus()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        var status = await _gameService.GetGameStatusAsync(session.SessionId);
        
        Assert.Equal(session.SessionId, status.SessionId);
        Assert.Equal("Alice", status.Player1Name);
        Assert.Equal("Bob", status.Player2Name);
        Assert.Equal(0, status.RoundsPlayed);
        Assert.Equal(1, status.CurrentRound);
        Assert.True(status.WaitingForPlayer1Choice);
        Assert.True(status.WaitingForPlayer2Choice);
        Assert.False(status.RoundComplete);
    }
    
    [Fact]
    public async Task GetGameStatus_WithPendingPlayer1Choice_ShouldShowWaitingForPlayer2()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
        var status = await _gameService.GetGameStatusAsync(session.SessionId);
        
        Assert.Equal(1, status.CurrentRound);
        Assert.False(status.WaitingForPlayer1Choice);
        Assert.True(status.WaitingForPlayer2Choice);
        Assert.False(status.RoundComplete);
    }
    
    [Fact]
    public async Task PendingRound_ShouldShowCorrectRoundNumber()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        // First round - should be round 1
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
        var sessionAfterFirstChoice = await _gameService.GetGameAsync(session.SessionId);
        Assert.NotNull(sessionAfterFirstChoice!.PendingRound);
        Assert.Equal(1, sessionAfterFirstChoice.PendingRound.RoundNumber);
        
        // Complete first round
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player2.PlayerId, Choice.Defect);
        
        // Start second round - should be round 2
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Defect);
        var sessionAfterSecondRound = await _gameService.GetGameAsync(session.SessionId);
        Assert.NotNull(sessionAfterSecondRound!.PendingRound);
        Assert.Equal(2, sessionAfterSecondRound.PendingRound.RoundNumber);
    }
    
    [Fact]
    public async Task GameStatus_ShouldShowCorrectCurrentRound()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        // Initial status - should be round 1
        var initialStatus = await _gameService.GetGameStatusAsync(session.SessionId);
        Assert.Equal(1, initialStatus.CurrentRound);
        Assert.Equal(0, initialStatus.RoundsPlayed);
        
        // Complete first round
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player2.PlayerId, Choice.Defect);
        
        // Status after first round - should be round 2
        var statusAfterFirstRound = await _gameService.GetGameStatusAsync(session.SessionId);
        Assert.Equal(2, statusAfterFirstRound.CurrentRound);
        Assert.Equal(1, statusAfterFirstRound.RoundsPlayed);
        
        // Start second round
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Defect);
        
        // Status during second round - should still be round 2
        var statusDuringSecondRound = await _gameService.GetGameStatusAsync(session.SessionId);
        Assert.Equal(2, statusDuringSecondRound.CurrentRound);
        Assert.Equal(1, statusDuringSecondRound.RoundsPlayed);
    }
    
    [Fact]
    public async Task SetPlayerChoice_WhenMaxRoundsReached_ShouldThrowException()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        // Play 157 rounds (maximum allowed)
        for (int i = 1; i <= 157; i++)
        {
            await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
            await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player2.PlayerId, Choice.Defect);
        }
        
        // Verify we have played exactly 157 rounds
        var gameSession = await _gameService.GetGameAsync(session.SessionId);
        Assert.Equal(157, gameSession!.Results.Count);
        
        // Attempting to start round 158 should throw an exception
        await Assert.ThrowsAsync<InvalidGameStateException>(() => 
            _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate));
    }
    
    [Fact]
    public async Task SetPlayerChoice_AtExactly157Rounds_ShouldWork()
    {
        var session = await _gameService.CreateGameAsync("Alice", "Bob");
        
        // Play exactly 156 complete rounds
        for (int i = 1; i <= 156; i++)
        {
            await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
            await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player2.PlayerId, Choice.Defect);
        }
        
        // Round 157 should still be allowed
        await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate);
        var result = await _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player2.PlayerId, Choice.Defect);
        
        // Should complete round 157 successfully
        Assert.NotNull(result);
        
        // Verify we now have exactly 157 rounds
        var gameSession = await _gameService.GetGameAsync(session.SessionId);
        Assert.Equal(157, gameSession!.Results.Count);
        
        // Now trying round 158 should fail
        await Assert.ThrowsAsync<InvalidGameStateException>(() => 
            _gameService.SetPlayerChoiceAsync(session.SessionId, session.Player1.PlayerId, Choice.Cooperate));
    }
    
    [Fact]
    public void GameController_ShouldRequireApiKeySettings()
    {
        // Arrange
        var apiKeySettings = new ApiKeySettings { MasterKey = "test-key" };
        var options = Options.Create(apiKeySettings);
        
        // Act & Assert - Should not throw when creating controller with valid settings
        var controller = new GameController(_gameService, options);
        Assert.NotNull(controller);
    }
    
    [Fact]
    public void JwtSettings_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var settings = new JwtSettings();
        
        // Assert
        Assert.False(settings.Enabled);
        Assert.Equal(60, settings.ExpirationMinutes);
        Assert.True(settings.ValidateIssuer);
        Assert.True(settings.ValidateAudience);
        Assert.True(settings.ValidateLifetime);
        Assert.True(settings.ValidateIssuerSigningKey);
        Assert.Empty(settings.Issuer);
        Assert.Empty(settings.Audience);
        Assert.Empty(settings.SecretKey);
    }
    
    [Fact]
    public void ConditionalAuthorizeAttribute_ShouldBeApplicable()
    {
        // Arrange & Act
        var attribute = new ConditionalAuthorizeAttribute();
        
        // Assert
        Assert.NotNull(attribute);
        Assert.IsAssignableFrom<Attribute>(attribute);
    }
}