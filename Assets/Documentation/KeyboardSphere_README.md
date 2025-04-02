# KeyboardSphere

The KeyboardSphere is an interactive object that can be controlled using the keyboard arrow keys.

## Setup Instructions

1. Open the Obstacles scene located in Assets/Scenes/Obstacles.unity
2. The KeyboardController will be automatically attached to the KeyboardSphere when the scene starts
3. Alternatively, in the Unity Editor menu, go to GameObject > Setup Keyboard Controller

## Controls

- **Up Arrow**: Move the sphere forward
- **Down Arrow**: Move the sphere backward
- **Left Arrow**: Move the sphere left
- **Right Arrow**: Move the sphere right

## Features

- Physics-based movement with configurable speed and drag
- Height constraint to keep the sphere at a consistent height
- Compatible with the ConcentricRings component for visual effects

## Additional Setup Options

### Adding Concentric Rings

To add visual concentric rings around the KeyboardSphere:

1. In the Unity Editor menu, go to GameObject > Setup Concentric Rings
2. This will automatically attach the ConcentricRings component to the KeyboardSphere

## Customization

You can customize the KeyboardController behavior by adjusting the following properties:

- **Move Speed**: Controls how quickly the sphere accelerates
- **Max Speed**: Limits the maximum velocity of the sphere
- **Use Physics**: Toggles between physics-based and transform-based movement
- **Height Constraint**: Keeps the sphere at a specific height (set to 0 to disable)
- **Drag**: Controls how quickly the sphere slows down when no input is provided