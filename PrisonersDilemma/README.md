# Prisoner's Dilemma API

A .NET Web API implementation of the classic Prisoner's Dilemma game theory scenario.

## Game Rules

In the Prisoner's Dilemma, two players must choose between "Cooperate" or "Defect" without knowing the other's choice. The scoring system is:

- **Both Cooperate**: 3 points each
- **Both Defect**: 1 point each  
- **One Cooperates, One Defects**: Defector gets 5 points, Cooperator gets 0 points

## API Endpoints

### Start Game
```
POST /api/game/start
```
Creates a new game session for a player to join.

**Request Body:**
```json
{
  "playerId": "guid",
  "playerName": "Alice"
}
```

**Response:**
```json
{
  "sessionId": "guid",
  "playerId": "guid", 
  "playerName": "Alice"
}
```

### Get Game Information
```
GET /api/game/{sessionId}
```
Retrieves the current state and information of a game session.

**Response:**
```json
{
  "sessionId": "guid",
  "status": "WaitingForPlayers | Active | Completed",
  "player1Name": "Alice",
  "player2Name": "Bob",
  "currentRound": 1,
  "summary": {
    "Alice": 15,
    "Bob": 12
  }
}
```

### Get Round Information
```
GET /api/game/{sessionId}/round/{roundNumber}
```
Retrieves information about a specific round in the game.

**Response:**
```json
{
  "sessionId": "guid",
  "roundNumber": 1,
  "status": "WaitingForChoices | Complete",
  "outcome": {
    "Alice": "Cooperate",
    "Bob": "Defect",
    "result": "Bob wins this round"
  },
  "summary": {
    "Alice": 0,
    "Bob": 5
  }
}
```

### Submit Player Choice
```
POST /api/game/choice
```
Allows a player to submit their choice for the current round.

**Request Body:**
```json
{
  "sessionId": "guid",
  "playerId": "guid",
  "roundNumber": 1,
  "choice": "Cooperate"
}
```

**Response:**
```json
{
  "sessionId": "guid",
  "roundNumber": 1,
  "status": "WaitingForChoices | Complete",
  "outcome": {
    "Alice": "Cooperate",
    "Bob": "Defect",
    "result": "Bob wins this round"
  },
  "summary": {
    "Alice": 0,
    "Bob": 5
  }
}
```

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