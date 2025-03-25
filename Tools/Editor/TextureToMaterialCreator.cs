using UnityEditor;
using UnityEngine;

public static class TextureToMaterialCreator
{
    [MenuItem("Assets/Create/Create Material for Textures", true)]
    private static bool ValidateCreateMaterialForTextures()
    {
        // Valida se o item do menu deve aparecer (apenas quando texturas estão selecionadas)
        foreach (var obj in Selection.objects)
        {
            if (!(obj is Texture)) return false;
        }
        return true;
    }

    [MenuItem("Assets/Create/Create Material for Textures")]
    private static void CreateMaterialForTextures()
    {
        foreach (var obj in Selection.objects)
        {
            if (obj is Texture texture)
            {
                // Cria o material
                string texturePath = AssetDatabase.GetAssetPath(texture);
                string materialPath = texturePath.Replace(".png", ".mat").Replace(".jpg", ".mat").Replace(".tga", ".mat");
                Material material = new Material(Shader.Find("Standard"));
                material.mainTexture = texture;

                // Salva o material no mesmo local que a textura
                AssetDatabase.CreateAsset(material, materialPath);
                Debug.Log($"Material created: {materialPath}");
            }
        }

        // Atualiza o banco de dados de assets na Unity
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
