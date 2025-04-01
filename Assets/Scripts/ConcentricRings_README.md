# Concentric Rings

This script creates three concentric rings around a target sphere (by default, the KeyboardSphere).

## Setup Instructions

1. Attach the `ConcentricRings` script to the KeyboardSphere GameObject.
2. Configure the ring settings in the Inspector:
   - **Min Distance To First Ring**: The minimum distance between the sphere's surface and the first ring (default: 1.0)
   - **Ring Spacing**: The distance between each ring (default: 1.0)
   - **Ring Thickness**: The thickness of each ring tube (default: 0.1)
   - **Ring Materials**: Array of materials to apply to each ring (optional)
   - **Target Sphere**: Reference to the sphere to surround (defaults to the GameObject this script is attached to)

## Creating Materials

You can create default materials for the rings using the provided editor utility:

1. In the Unity Editor, go to GameObject > Create Ring Materials
2. This will create three colored materials in the Assets/Materials folder:
   - RingMaterial1 (Orange)
   - RingMaterial2 (Blue)
   - RingMaterial3 (Green)
3. Assign these materials to the Ring Materials array in the ConcentricRings component

## Features

- Rings automatically follow the sphere as it moves
- Ring sizes are calculated based on the sphere's collider radius
- Each ring can have a different material
- Rings are generated procedurally at runtime

## Customization

You can modify the appearance of the rings by:
- Changing the materials
- Adjusting the spacing and thickness parameters
- Modifying the number of segments in the CreateTorusMesh method for higher/lower detail

## Technical Details

The rings are created using procedurally generated torus meshes. Each ring is a separate GameObject with its own MeshFilter, MeshRenderer, and MeshCollider components.