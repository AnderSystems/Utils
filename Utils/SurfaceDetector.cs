using UnityEngine;

public class SurfaceDetector : MonoBehaviour
{
    public struct Surface
    {
        /// <summary>
        /// The tag returns the name of layer (in case of terrain) or the tag of object (in case of object)
        /// </summary>
        public string tag;
        public Texture2D texture;
        public Material material;
    }

    public static Surface GetSurfaceOnHit(RaycastHit hit)
    {
        Surface surface = new Surface();

        if (hit.collider != null)
        {
            // Se for um terreno, obtém a textura do ponto de impacto
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                TerrainLayer tl = GetLayerOnPoint(terrain, hit.point);
                surface.material = terrain.materialTemplate;
                surface.texture = tl.diffuseTexture;
                surface.tag = tl.name;
            }
            else
            {
                // Para outros objetos, pega o material exato e a textura
                surface.material = GetMaterialOnHit(hit);
                if (surface.material != null && surface.material.mainTexture is Texture2D tex)
                {
                    surface.texture = tex;
                    surface.tag = hit.collider.tag;
                }
            }
        }

        return surface;
    }

    public static TerrainLayer GetLayerOnPoint(Terrain terrain, Vector3 worldCoord)
    {
        if (terrain == null || terrain.terrainData == null)
            return null;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // Converte coordenadas do mundo para coordenadas do terreno
        int mapX = Mathf.FloorToInt((worldCoord.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth);
        int mapZ = Mathf.FloorToInt((worldCoord.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight);

        float[,,] alphaMaps = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
        int maxIndex = 0;
        float maxAlpha = 0f;

        // Encontra a textura dominante no ponto de impacto
        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            if (alphaMaps[0, 0, i] > maxAlpha)
            {
                maxAlpha = alphaMaps[0, 0, i];
                maxIndex = i;
            }
        }

        return terrainData.terrainLayers[maxIndex];
    }

    public static Material GetMaterialOnHit(RaycastHit hit)
    {
        Renderer renderer = hit.collider.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterials.Length > 0)
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null)
            {
                Mesh mesh = meshCollider.sharedMesh;
                int[] triangles = mesh.triangles;
                int materialIndex = triangles[hit.triangleIndex] / 3;
                if (materialIndex < renderer.sharedMaterials.Length)
                {
                    return renderer.sharedMaterials[materialIndex];
                }
            }
            return renderer.sharedMaterial;
        }
        return null;
    }
}