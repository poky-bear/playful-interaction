using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DarkRingMaterialCreator : MonoBehaviour
{
    // This script is used to create dark materials for the rings
    // It's only used in the editor and won't be included in the build
    
#if UNITY_EDITOR
    [MenuItem("GameObject/Create Dark Ring Materials")]
    public static void CreateDarkRingMaterials()
    {
        // Create materials folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        
        // Create dark material
        CreateMaterial("DarkRingMaterial", new Color(0.2f, 0.2f, 0.2f, 1f)); // Dark gray
        
        // Create bright material
        CreateMaterial("BrightRingMaterial", new Color(1f, 0.8f, 0.2f, 1f)); // Bright yellow
        
        AssetDatabase.Refresh();
    }
    
    private static void CreateMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        
        // Check if material already exists
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
        {
            Debug.Log("Material " + name + " already exists.");
            return;
        }
        
        // Create new material
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        
        if (name.Contains("Bright"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.5f);
        }
        
        // Save material as asset
        AssetDatabase.CreateAsset(material, path);
        Debug.Log("Created material: " + path);
    }
#endif
}