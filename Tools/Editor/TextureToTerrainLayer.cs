using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureToTerrainLayer : MonoBehaviour
{
    [MenuItem("Assets/Create/Terrain/Convert Textures to Terrain Layers", false, 1000)]
    public static void ConvertTexturesToTerrainLayers()
    {
        // Get selected assets
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No textures selected. Please select one or more textures.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            if (obj is Texture2D texture)
            {
                CreateTerrainLayer(texture);
            }
            else
            {
                Debug.LogWarning($"{obj.name} is not a Texture2D. Skipping.");
            }
        }
    }

    private static void CreateTerrainLayer(Texture2D texture)
    {
        // Determine save path
        string texturePath = AssetDatabase.GetAssetPath(texture);
        string directory = Path.GetDirectoryName(texturePath);
        string terrainLayerPath = Path.Combine(directory, "TerrainLayers", texture.name + ".terrainlayer");

        // Create TerrainLayer
        TerrainLayer terrainLayer = new TerrainLayer
        {
            diffuseTexture = texture
        };

        // Save the TerrainLayer as an asset
        AssetDatabase.CreateAsset(terrainLayer, terrainLayerPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created Terrain Layer: {terrainLayerPath}");
    }
}
