# Player 2 Setup Instructions

This README explains how to add a second player to the Concentric Rings game.

## Automatic Setup

The easiest way to add a second player is to use the automatic setup tool:

1. Open your scene in the Unity Editor
2. Go to the menu: GameObject > Setup Player 2
3. This will automatically create:
   - A second sphere with the necessary components
   - Concentric rings around the second sphere
   - UI elements for the second player

## Manual Setup

If you prefer to set up the second player manually:

1. Create a new sphere in your scene (name it "Player2Sphere" for clarity)
2. Add the following components to the sphere:
   - ConcentricRings
   - Player2RingGameController
   - Player2Controller
3. Create a UI Canvas (or use an existing one) and add:
   - Text elements for instructions, status, and feedback
   - Add Player2RingGameUI component to the canvas or a panel within it

## Player 2 Controls

Player 2 uses different controls than Player 1:

- **Movement**: IJKL keys (instead of arrow keys)
- **Action**: F key (instead of spacebar)
- **Reset**: R key (same as Player 1)

You can customize these controls in the Inspector by modifying the Player2Controller component.

## Customization

You can customize various aspects of Player 2 by adjusting the public properties in the Inspector:

- **Ring Settings**: Spacing, thickness, and distance from sphere
- **Game Settings**: 
  - Colors: Dark color for inactive rings, bright color for the active ring
  - Expansion Speed: How quickly the dark circle expands from the center
  - Hit Tolerance: The error margin for hitting the target ring
- **Movement Settings**: Speed, drag, and physics behavior

## Troubleshooting

If you encounter issues with the second player:

1. Make sure all required components are attached to the Player 2 sphere
2. Check that the UI elements are properly connected to the Player2RingGameUI component
3. Verify that the ConcentricRings component has initialized properly

## Script Overview

- **Player2Controller.cs**: Controls sphere movement with WASD keys
- **Player2RingGameController.cs**: Handles the core game logic for Player 2
- **Player2RingGameUI.cs**: Manages the UI elements for Player 2
- **Player2Setup.cs**: Editor script to automatically set up Player 2