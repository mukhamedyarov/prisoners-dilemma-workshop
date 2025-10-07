using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PrisonersDilemma.Api.Validation;

namespace PrisonersDilemma.Api.Models;

public record GameResult(
    int Player1Score,
    int Player2Score,
    Choice Player1Choice,
    Choice Player2Choice,
    string Outcome);

public record PendingRound(
    int RoundNumber,
    bool IsComplete);

public record GameSession(
    Guid SessionId,
    string Player1Name,
    string Player2Name,
    List<GameResult> Results,
    PendingRound? PendingRound,
    DateTime CreatedAt);

public record CreateGameRequest(
    [Required, StringLength(100, MinimumLength = 1, ErrorMessage = "Player name must be between 1 and 100 characters")]
    string Player1Name,
    
    [Required, StringLength(100, MinimumLength = 1, ErrorMessage = "Player name must be between 1 and 100 characters")]
    string Player2Name)
{
    public static implicit operator (string, string)(CreateGameRequest request) =>
        (request.Player1Name, request.Player2Name);
};

public record Player(
    [Required, NotEmptyGuid]
    Guid PlayerId,
    
    [Required]
    string PlayerName);

public record CreateGameResult(
    Guid SessionId,
    Player Player1,
    Player Player2);

public record SetPlayerChoiceRequest(
    [Required, NotEmptyGuid]
    Guid PlayerId,
    
    [Required]
    Choice Choice);

public record GameStatus(
    Guid SessionId,
    string Player1Name,
    string Player2Name,
    int RoundsPlayed,
    int CurrentRound,
    bool WaitingForPlayer1Choice,
    bool WaitingForPlayer2Choice,
    bool RoundComplete);