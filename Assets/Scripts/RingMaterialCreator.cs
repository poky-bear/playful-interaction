using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RingMaterialCreator : MonoBehaviour
{
    // This script is used to create default materials for the rings
    // It's only used in the editor and won't be included in the build
    
#if UNITY_EDITOR
    [MenuItem("GameObject/Create Ring Materials")]
    public static void CreateRingMaterials()
    {
        // Create materials folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        
        // Create three materials with different colors
        CreateMaterial("RingMaterial1", new Color(1f, 0.5f, 0f, 1f)); // Orange
        CreateMaterial("RingMaterial2", new Color(0f, 0.7f, 1f, 1f)); // Blue
        CreateMaterial("RingMaterial3", new Color(0.5f, 1f, 0.3f, 1f)); // Green
        
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
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * 0.5f);
        
        // Save material as asset
        AssetDatabase.CreateAsset(material, path);
        Debug.Log("Created material: " + path);
    }
#endif
}