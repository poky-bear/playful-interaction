# Multiplayer Ring Game

This feature adds a cooperative multiplayer mode to the ring game where both players must work together to complete the challenge.

## How It Works

1. **Activation**: When Player 1 and Player 2 come within close proximity of each other for 3 seconds, the multiplayer mode activates.

2. **Gameplay**:
   - A set of purple rings appears between the two players.
   - Both players must press and hold their action buttons simultaneously (Space for Player 1, F for Player 2).
   - When both buttons are pressed, an expanding circle grows from the center.
   - Both players must release their buttons at the same time to try to match the active purple ring.
   - If successful, the next ring becomes active.
   - Complete all three rings to win the challenge.

3. **Deactivation**: If players move too far apart, the multiplayer mode deactivates.

## Setup Instructions

1. Make sure you have both Player 1 and Player 2 set up in your scene:
   - Player 1 should be a sphere named "Sphere" with a RingGameController component.
   - Player 2 should be a sphere named "Player2Sphere" with a Player2RingGameController component.

2. In the Unity Editor, go to GameObject > Setup Multiplayer Mode.
   - This will create a MultiplayerController GameObject with the necessary components.
   - It will also set up the UI for the multiplayer mode.

## Controls

- **Player 1**: 
  - Movement: Arrow keys
  - Action: Space bar

- **Player 2**: 
  - Movement: WASD keys
  - Action: F key

- **Both Players**:
  - Reset: R key

## Technical Details

The multiplayer system consists of the following components:

1. **MultiplayerRingGame.cs**: The main controller that manages the multiplayer gameplay.
2. **MultiplayerRingGameUI.cs**: Handles the UI display for the multiplayer mode.
3. **MultiplayerSetup.cs**: Editor script to easily set up the multiplayer mode in a scene.

The system detects when players are close to each other, creates a shared ring game between them, and requires synchronized button presses to progress through the rings.