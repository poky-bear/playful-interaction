using UnityEngine;
using UnityEditor;

public class SetupConcentricRings : EditorWindow
{
    [MenuItem("GameObject/Setup Concentric Rings")]
    public static void SetupRings()
    {
        // Find the KeyboardSphere
        GameObject keyboardSphere = GameObject.Find("KeyboardSphere");
        if (keyboardSphere == null)
        {
            return;
        }
        
        // Add ConcentricRings component if it doesn't exist
        ConcentricRings rings = keyboardSphere.GetComponent<ConcentricRings>();
        if (rings == null)
        {
            rings = keyboardSphere.AddComponent<ConcentricRings>();
        }
        
        // Set default values
        rings.minDistanceToFirstRing = 1.0f;
        rings.ringSpacing = 1.0f;
        rings.ringThickness = 0.1f;
        rings.targetSphere = keyboardSphere;
        
        // Try to find and assign materials
        rings.ringMaterials = new Material[3];
        rings.ringMaterials[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RingMaterial1.mat");
        rings.ringMaterials[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RingMaterial2.mat");
        rings.ringMaterials[2] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RingMaterial3.mat");
        
        // Create materials if they don't exist
        if (rings.ringMaterials[0] == null || rings.ringMaterials[1] == null || rings.ringMaterials[2] == null)
        {
            RingMaterialCreator.CreateRingMaterials();
            
            // Try loading the materials again
            rings.ringMaterials[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RingMaterial1.mat");
            rings.ringMaterials[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RingMaterial2.mat");
            rings.ringMaterials[2] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RingMaterial3.mat");
        }
        
    }
}