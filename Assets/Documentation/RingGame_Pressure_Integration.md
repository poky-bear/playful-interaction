# Ring Game Pressure Sensor Integration

This document explains how to set up and use the ESP32C3 pressure sensor with the Ring Game in Unity.

## Overview

The Ring Game is a timing-based game where a ring expands and contracts continuously. The player must apply pressure to the sensor at the right moment to stop the ring when it aligns with a target zone. This is similar to pressing the space bar in the keyboard version of the game.

## Hardware Setup

Follow the general ESP32C3 setup instructions in the [ESP32C3_Pressure_Integration.md](ESP32C3_Pressure_Integration.md) document for connecting your pressure sensor to the ESP32C3 microcontroller.

## Game Mechanics

1. **Ring Movement**: The inner ring automatically expands and contracts between a minimum and maximum size.
2. **Target Zone**: A green zone represents the target area where the player should try to stop the ring.
3. **Pressure Control**: Applying pressure above the threshold (default: 30) will stop the ring, similar to pressing the space bar.
4. **Scoring**:
   - Perfect alignment with the target zone: +10 points
   - Close to the target zone: +5 points
   - Far from the target zone: 0 points

## Setting Up the Ring Game

### Option 1: Using the RingGameSetup Script

1. Create an empty GameObject in your scene
2. Add the `RingGameSetup.cs` script to this GameObject
3. Call the `SetupRingGame()` method to automatically create the ring game UI

```csharp
// Example: Call this from another script
FindObjectOfType<RingGameSetup>().SetupRingGame();
```

### Option 2: Manual Setup

1. Create a Canvas in your scene (if one doesn't already exist)
2. Create the following UI elements as children of the Canvas:
   - OuterRing (Image, circle shape)
   - TargetZone (Image, partial circle or rectangle)
   - InnerRing (Image, circle shape)
   - ScoreText (Text)
   - FeedbackText (Text)
3. Create an empty GameObject and add the `PressureGameController.cs` script
4. Assign the UI elements to the corresponding fields in the Inspector
5. Create another empty GameObject and add the `ESP32BluetoothManager.cs` script
6. Assign the ESP32BluetoothManager to the PressureGameController

## Customizing the Game

You can adjust the following parameters in the PressureGameController script:

- **ringSpeed**: How fast the ring expands and contracts
- **minRingScale** and **maxRingScale**: The minimum and maximum sizes of the ring
- **targetZoneSize**: The width of the target zone
- **targetZonePosition**: The position of the target zone (between min and max scale)
- **pressureThreshold**: The pressure value (0-100) that triggers the "pressed" state

## Testing Without Hardware

For testing without the actual pressure sensor hardware:

1. The game includes keyboard input support - press the Space bar to simulate applying pressure
2. The ESP32BluetoothManager script includes a simulation mode that generates random pressure values

## Troubleshooting

### Ring Not Responding to Pressure

1. Check the console logs to see if pressure values are being received
2. Verify that the pressure threshold is set appropriately (default is 30)
3. Make sure the ESP32BluetoothManager is correctly assigned to the PressureGameController

### Visual Elements Not Appearing Correctly

1. Check that all UI elements are properly assigned in the Inspector
2. Verify that the Canvas is set to "Screen Space - Overlay" mode
3. Make sure the RectTransform settings for each UI element are appropriate

### Bluetooth Connection Issues

Refer to the troubleshooting section in the [ESP32C3_Pressure_Integration.md](ESP32C3_Pressure_Integration.md) document for Bluetooth connectivity issues.