# Prisoner's Dilemma API

A .NET Web API implementation of the classic Prisoner's Dilemma game theory scenario.

## Game Rules

In the Prisoner's Dilemma, two players must choose between "Cooperate" or "Defect" without knowing the other's choice. The scoring system is:

- **Both Cooperate**: 3 points each
- **Both Defect**: 1 point each  
- **One Cooperates, One Defects**: Defector gets 5 points, Cooperator gets 0 points

## API Endpoints

### Create Game
```
POST /api/game/create
```
Creates a new game session between two players.

**Request Body:**
```json
{
  "player1Name": "Alice",
  "player2Name": "Bob"
}
```

**Response:**
```json
{
  "sessionId": "guid",
  "player1Name": "Alice",
  "player2Name": "Bob",
  "results": [],
  "pendingRound": null,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Set Player Choice (New - Separate Choices)
```
POST /api/game/choice
```
Allows each player to set their choice separately without knowing the other player's choice.

**Request Body:**
```json
{
  "sessionId": "guid",
  "playerNumber": 1,
  "choice": "Cooperate"
}
```

**Response (First Player):**
```json
{
  "roundComplete": false,
  "status": {
    "sessionId": "guid",
    "player1Name": "Alice",
    "player2Name": "Bob",
    "roundsPlayed": 0,
    "waitingForPlayer1Choice": false,
    "waitingForPlayer2Choice": true,
    "roundComplete": false
  }
}
```

**Response (Second Player - Round Complete):**
```json
{
  "roundComplete": true,
  "result": {
    "player1Score": 0,
    "player2Score": 5,
    "player1Choice": "Cooperate",
    "player2Choice": "Defect",
    "outcome": "Player 1 cooperated, Player 2 defected - Player 2 wins"
  }
}
```

### Get Game Status
```
GET /api/game/{sessionId}/status
```
Gets the current status of the game, including which players need to make choices.

**Response:**
```json
{
  "sessionId": "guid",
  "player1Name": "Alice",
  "player2Name": "Bob",
  "roundsPlayed": 2,
  "waitingForPlayer1Choice": true,
  "waitingForPlayer2Choice": true,
  "roundComplete": false
}
```

### Play Round (Legacy - Both Choices Together)
```
POST /api/game/play
```
Plays a single round of the game with both choices provided at once.

**Request Body:**
```json
{
  "sessionId": "guid",
  "player1Choice": "Cooperate",
  "player2Choice": "Defect"
}
```

**Response:**
```json
{
  "player1Score": 0,
  "player2Score": 5,
  "player1Choice": "Cooperate",
  "player2Choice": "Defect",
  "outcome": "Player 1 cooperated, Player 2 defected - Player 2 wins"
}
```

### Get Game
```
GET /api/game/{sessionId}
```
Retrieves the current state of a game session.

### Get Game Summary
```
GET /api/game/{sessionId}/summary
```
Gets a summary of the game including total scores and winner.

**Response:**
```json
{
  "sessionId": "guid",
  "player1Name": "Alice",
  "player2Name": "Bob",
  "roundsPlayed": 3,
  "totalPlayer1Score": 8,
  "totalPlayer2Score": 6,
  "winner": "Alice",
  "results": [...]
}
```

### Get All Games
```
GET /api/game/all
```
Retrieves all game sessions.

## Development

### Database Setup
The application uses SQLite for data persistence. The database file (`PrisonersDilemma.db`) will be created automatically when you first run the application.

**Connection String**: `Data Source=PrisonersDilemma.db`

### Build and Run
```bash
dotnet build
dotnet run --project PrisonersDilemma.Api
```

The database will be created automatically on first run. Game sessions and results are persisted permanently.

### Run Tests
```bash
dotnet test --settings UnitTests.runsettings
```

Tests use an in-memory database for isolation and performance.

### Database Migrations
If you need to modify the database schema:
```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project PrisonersDilemma.Api

# Apply migrations
dotnet ef database update --project PrisonersDilemma.Api
```

### API Documentation
The API includes OpenAPI/Swagger documentation available at:
- **Swagger UI**: `/swagger` (when running in development)  
- **OpenAPI JSON**: `/openapi/v1.json`

## Game Theory Context

The Prisoner's Dilemma illustrates why two rational individuals might not cooperate even when it's in their best interest to do so. It's a fundamental concept in game theory with applications in economics, politics, and psychology.