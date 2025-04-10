# Multiplayer Functionality

This update adds multiplayer support to the Concentric Rings Game. Here's what's been added:

## Features

1. **Second Player**
   - Uses WASD keys for movement
   - Uses F key to play the ring game
   - Has its own set of concentric rings

2. **Dynamic Boid System**
   - The number of boids scales with the number of players (12 boids per player)
   - When a player completes the ring game, only a maximum of 6 boids will follow that player

3. **Player Management**
   - A new PlayerManager component keeps track of all players and their game completion status

## How to Set Up Multiplayer

### Using the Editor Menu

1. Open your scene in the Unity Editor
2. Go to the menu: GameObject > Setup Second Player
3. This will automatically create a second player sphere with all necessary components

### Manual Setup

If you prefer to set up the second player manually:

1. Create a new sphere in your scene
2. Add the following components to the sphere:
   - WASDController
   - ConcentricRings
   - Player2RingGameController
3. Ensure there's a PlayerManager in your scene

## Controls

### Player 1
- Movement: Arrow keys
- Ring Game: Spacebar

### Player 2
- Movement: WASD keys
- Ring Game: F key

## Technical Details

### New Scripts

- **WASDController.cs**: Controls movement for the second player using WASD keys
- **Player2RingGameController.cs**: Modified version of RingGameController that uses the F key
- **PlayerManager.cs**: Manages player registration and boid allocation
- **MultiplayerSetup.cs**: Editor utility for easy setup of the second player

### Modified Scripts

- **RingGameController.cs**: Modified to work with the PlayerManager
- **Spawner.cs**: Modified to adjust boid count based on player count
- **Boid.cs**: Modified to limit the number of boids following completed players

## Future Improvements

- Support for more than two players
- Different colors for each player's rings
- Competitive or cooperative gameplay modes
- Scoreboard for tracking player performance