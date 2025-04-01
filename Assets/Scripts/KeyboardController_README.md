# Keyboard Controller for Unity

This script allows you to control a GameObject using the keyboard arrow keys.

## Setup Instructions

1. Select the `KeyboardSphere` GameObject in your scene hierarchy
2. Click "Add Component" in the Inspector
3. Search for "KeyboardController" and add it

## Configuration Options

### Movement Settings
- **Move Speed**: How fast the object accelerates (default: 10)
- **Max Speed**: Maximum velocity the object can reach (default: 15)
- **Use Physics**: If enabled, uses Rigidbody physics for movement (default: true)
- **Height Constraint**: If > 0, locks the object to this Y position (default: 0)

### Physics Settings
- **Drag**: Amount of drag applied to the Rigidbody (default: 0.5)

## Controls
- **Up Arrow**: Move forward (positive Z)
- **Down Arrow**: Move backward (negative Z)
- **Left Arrow**: Move left (negative X)
- **Right Arrow**: Move right (positive X)

## Notes
- If `Use Physics` is enabled but no Rigidbody is attached, one will be automatically added
- The script prevents rotation of the object when using physics