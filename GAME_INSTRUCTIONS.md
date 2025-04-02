# Concentric Rings Game

This Unity project implements a game with concentric rings around a sphere. The player needs to time the release of the spacebar to match an expanding circle with the highlighted ring.

## Game Features

1. Three concentric rings around a central sphere
2. Rings are initially dark, with a random order assigned at the start
3. The first ring in the order is highlighted with a bright color
4. When the player presses the spacebar, a dark circle expands from the center
5. If the player releases the spacebar when the expanding circle is near the bright ring (within the hit tolerance), the game progresses to the next ring
6. The game provides feedback on how close the player was to the perfect timing
7. The goal is to successfully time the release for all three rings in the assigned order

## Setup Instructions

### In the Unity Editor:

1. Open the project in Unity
2. Go to the menu: GameObject > Setup Ring Game
3. This will automatically create:
   - A main sphere with the necessary components
   - Concentric rings around the sphere
   - UI elements with instructions and status

### Manual Setup:

If you prefer to set up the game manually:

1. Create a sphere in your scene
2. Add the following components to the sphere:
   - ConcentricRings
   - RingGameController
   - KeyboardController (optional, for movement)
3. Create a UI Canvas with:
   - Text elements for instructions and status
   - Add RingGameUI component to the canvas

## How to Play

1. Use arrow keys to move the sphere (optional)
2. Press and hold the SPACEBAR to start expanding the dark circle
3. Release the SPACEBAR when the expanding circle reaches the bright ring
4. If timed correctly, the current ring turns dark and the next ring in the sequence becomes bright
5. Complete all three rings to win
6. Press R to reset the game at any time

## Scripts Overview

- **ConcentricRings.cs**: Creates and manages the three rings around the sphere
- **RingGameController.cs**: Handles the core game logic, including ring order, expanding circle, and hit detection
- **RingGameUI.cs**: Manages the UI elements and displays game status
- **KeyboardController.cs**: Controls sphere movement with arrow keys
- **RingGameSetup.cs**: Editor script to automatically set up the game

## Customization

You can customize various aspects of the game by adjusting the public properties in the Inspector:

- **Ring Settings**: Spacing, thickness, and distance from sphere
- **Game Settings**: 
  - Colors: Dark color for inactive rings, bright color for the active ring
  - Expansion Speed: How quickly the dark circle expands from the center
  - Hit Tolerance: The error margin for hitting the target ring (default: 0.3 units)
    - Smaller values (e.g., 0.1) make the game more challenging
    - Larger values (e.g., 0.5) make it more forgiving
    - The game provides feedback on how close the player was to the perfect hit
- **Movement Settings**: Speed, drag, and physics behavior

## Difficulty Adjustment

To adjust the game's difficulty:

1. **Hit Tolerance**: The primary difficulty setting
   - Easy: 0.5 units or higher
   - Medium: 0.3 units (default)
   - Hard: 0.1 units or lower

2. **Expansion Speed**: Affects how quickly the player needs to react
   - Slower speeds (1.0-1.5) make timing easier
   - Faster speeds (2.5-3.0) make timing more challenging

3. **Ring Spacing**: Adjust in the ConcentricRings component
   - Closer rings increase difficulty
   - More widely spaced rings make the game easier

Enjoy the game!