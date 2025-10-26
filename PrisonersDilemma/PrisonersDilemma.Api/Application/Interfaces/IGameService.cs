using PrisonersDilemma.Api.Application.DTOs;

namespace PrisonersDilemma.Api.Application.Interfaces;

public interface IGameService
{
	Task<GameInfoResponse> GetGameInfoAsync(Guid sessionId);
	Task<RoundInfoResponse> GetRoundInfoAsync(Guid sessionId, int roundNumber);
	Task<StartGameResponse> StartGameAsync(StartGameRequest request);
	Task<RoundInfoResponse> SubmitChoiceAsync(SubmitChoiceRequest request);
}