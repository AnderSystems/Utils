using System.Collections.Generic;
using UnityEngine;
using System.Linq;




#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TerrainFlatten))]
public class TerrainManipulatorEditor : Editor
{
    private Dictionary<GameObject, (float[,], Vector3, List<TreePrototype>, List<DetailPrototype>)> terrainHistory = new Dictionary<GameObject, (float[,], Vector3, List<TreePrototype>, List<DetailPrototype>)>();

    public override void OnInspectorGUI()
    {
        TerrainFlatten manipulator = (TerrainFlatten)target;
        DrawDefaultInspector();

        Terrain terrain = GetTerrainUnderObject(manipulator.gameObject);
        if (terrain == null)
        {
            EditorGUILayout.HelpBox("No terrain found under the object.", MessageType.Error);
            return;
        }

        if (GUILayout.Button("Flatten Terrain"))
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Flatten Terrain");
            ApplyFlattening(manipulator, terrain);
        }

        if (GUILayout.Button("Restore Terrain"))
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Restore Terrain");
            RestoreTerrain(manipulator.gameObject, terrain);
        }

        if (GUILayout.Button("Remove Trees"))
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Remove Trees");
            RemoveTrees(manipulator, terrain);
        }

        if (GUILayout.Button("Remove Grass and Details"))
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Remove Grass and Details");
            RemoveGrassAndDetails(manipulator, terrain);
        }
    }

    private void OnSceneGUI()
    {
        TerrainFlatten manipulator = (TerrainFlatten)target;
        if (manipulator == null) return;

        // Desenha o círculo para visualizar o raio do flatten
        Handles.color = Color.green;
        Handles.DrawWireDisc(manipulator.transform.position, Vector3.up, manipulator.flattenRadius);
    }

    private Terrain GetTerrainUnderObject(GameObject obj)
    {
        Terrain[] terrains = Terrain.activeTerrains;
        foreach (Terrain terrain in terrains)
        {
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            if (obj.transform.position.x >= terrainPos.x &&
                obj.transform.position.x <= terrainPos.x + terrainSize.x &&
                obj.transform.position.z >= terrainPos.z &&
                obj.transform.position.z <= terrainPos.z + terrainSize.z)
            {
                return terrain;
            }
        }
        return null;
    }

    private void ApplyFlattening(TerrainFlatten manipulator, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        GameObject obj = manipulator.gameObject;
        Vector3 objPosition = obj.transform.position;
        float objHeight = obj.transform.position.y / terrainData.size.y;

        if (!terrainHistory.ContainsKey(obj) || terrainHistory[obj].Item2 != objPosition)
        {
            Rect affectedArea = GetTerrainAffectedArea(objPosition, manipulator.flattenRadius, terrain);
            terrainHistory[obj] = (GetTerrainHeights(terrain, affectedArea), objPosition, new List<TreePrototype>(terrainData.treePrototypes), new List<DetailPrototype>(terrainData.detailPrototypes));
        }

        FlattenTerrain(terrain, objPosition, manipulator.flattenRadius, objHeight);
    }

    private void RestoreTerrain(GameObject obj, Terrain terrain)
    {
        if (!terrainHistory.ContainsKey(obj))
        {
            Debug.LogWarning("No saved terrain state for this object.");
            return;
        }

        Rect terrainRect = GetTerrainAffectedArea(obj.transform.position, ((TerrainFlatten)obj.GetComponent<TerrainFlatten>()).flattenRadius, terrain);
        ApplyTerrainHeights(terrain, terrainRect, terrainHistory[obj].Item1);

        // Restaurar as árvores e grama
        terrain.terrainData.treePrototypes = terrainHistory[obj].Item3.ToArray();
        terrain.terrainData.detailPrototypes = terrainHistory[obj].Item4.ToArray();

        terrainHistory.Remove(obj);
    }


    void RemoveDuplicatesTreesPrototype(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;

        // Lista para armazenar protótipos únicos de árvores
        List<TreePrototype> uniquePrototypes = new List<TreePrototype>();

        // Itera sobre os protótipos de árvores e remove duplicados
        foreach (var prototype in terrainData.treePrototypes)
        {
            if (!uniquePrototypes.Contains(prototype))
            {
                uniquePrototypes.Add(prototype);
            }
        }

        // Atualiza os prototypes no terreno com os protótipos únicos
        terrainData.treePrototypes = uniquePrototypes.ToArray();
    }



    private void RemoveTrees(TerrainFlatten manipulator, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;

        List<TreeInstance> treeInstances = terrainData.treeInstances.ToList();

        bool treesRemoved;

        // Repetir a remoção enquanto houver árvores próximas
        do
        {
            treesRemoved = false; // Flag para saber se houve remoção

            // Lista temporária para armazenar as instâncias de árvore que devem ser mantidas
            List<TreeInstance> remainingTrees = new List<TreeInstance>();

            for (int i = 0; i < treeInstances.Count; i++)
            {
                TreeInstance treeInstance = treeInstances[i];

                // Posição normalizada da árvore no terreno
                Vector3 treePositionNormalized = treeInstance.position;

                // Converte a posição normalizada para a posição global no mundo
                Vector3 treePositionGlobal = new Vector3(
                    terrain.transform.position.x + treePositionNormalized.x * terrainData.size.x,
                    terrain.transform.position.y + treePositionNormalized.y * terrainData.size.y,
                    terrain.transform.position.z + treePositionNormalized.z * terrainData.size.z
                );

                // Verifica a distância entre a árvore e o manipulador
                if (Vector3.Distance(treePositionGlobal, manipulator.transform.position) <= manipulator.flattenRadius)
                {
                    // Se a árvore estiver dentro do raio, ela será removida
                    treesRemoved = true;
                }
                else
                {
                    // Caso contrário, mantém a árvore na lista de instâncias
                    remainingTrees.Add(treeInstance);
                }
            }

            // Atualiza a lista de árvores
            treeInstances = remainingTrees;

        } while (treesRemoved); // Se alguma árvore foi removida, repete o processo

        // Atualiza as instâncias de árvores no terreno
        terrainData.SetTreeInstances(treeInstances.ToArray(), true);
    }







    private void RemoveGrassAndDetails(TerrainFlatten manipulator, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;

        // Para cada camada de detalhe (grama ou outros detalhes)
        for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
        {
            int[,] detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);

            bool detailsRemoved;

            // Repetir a remoção de detalhes enquanto houver detalhes próximos
            do
            {
                detailsRemoved = false;

                // Lista temporária para armazenar os detalhes que devem ser mantidos
                int[,] newDetailLayer = new int[terrainData.detailWidth, terrainData.detailHeight];

                for (int x = 0; x < terrainData.detailWidth; x++)
                {
                    for (int z = 0; z < terrainData.detailHeight; z++)
                    {
                        // Posição normalizada do detalhe
                        Vector3 detailPositionNormalized = new Vector3(
                            (float)x / terrainData.detailWidth,
                            0f,
                            (float)z / terrainData.detailHeight
                        );

                        // Converte a posição normalizada para a posição global no mundo
                        Vector3 detailPositionGlobal = new Vector3(
                            terrain.transform.position.x + detailPositionNormalized.x * terrainData.size.x,
                            terrain.transform.position.y + detailPositionNormalized.y * terrainData.size.y,
                            terrain.transform.position.z + detailPositionNormalized.z * terrainData.size.z
                        );

                        // Verifica a distância entre o detalhe e o manipulador
                        if (Vector3.Distance(detailPositionGlobal, manipulator.transform.position) <= manipulator.flattenRadius)
                        {
                            // Se o detalhe estiver dentro do raio de remoção, define como 0 (remover)
                            newDetailLayer[x, z] = 0;
                            detailsRemoved = true;
                        }
                        else
                        {
                            // Caso contrário, mantém o valor original
                            newDetailLayer[x, z] = detailLayer[x, z];
                        }
                    }
                }

                // Atualiza a camada de detalhes
                terrainData.SetDetailLayer(0, 0, i, newDetailLayer);

            } while (detailsRemoved); // Se algum detalhe foi removido, repete o processo
        }
    }



    private Rect GetTerrainAffectedArea(Vector3 center, float radius, Terrain terrain)
    {
        Vector3 terrainPos = terrain.transform.position;
        float resolution = terrain.terrainData.heightmapResolution / terrain.terrainData.size.x;

        return new Rect(
            (center.x - radius - terrainPos.x) * resolution,
            (center.z - radius - terrainPos.z) * resolution,
            radius * 2 * resolution,
            radius * 2 * resolution
        );
    }

    private float[,] GetTerrainHeights(Terrain terrain, Rect rect)
    {
        TerrainData data = terrain.terrainData;

        int xStart = Mathf.FloorToInt(rect.xMin);
        int zStart = Mathf.FloorToInt(rect.yMin);
        int width = Mathf.CeilToInt(rect.width);
        int height = Mathf.CeilToInt(rect.height);

        return data.GetHeights(xStart, zStart, width, height);
    }

    private void ApplyTerrainHeights(Terrain terrain, Rect rect, float[,] heights)
    {
        TerrainData data = terrain.terrainData;
        data.SetHeights(Mathf.FloorToInt(rect.xMin), Mathf.FloorToInt(rect.yMin), heights);
    }

    private void FlattenTerrain(Terrain terrain, Vector3 center, float radius, float height)
    {
        TerrainData data = terrain.terrainData;
        Rect affectedArea = GetTerrainAffectedArea(center, radius, terrain);

        int width = Mathf.CeilToInt(affectedArea.width);
        int heightMapHeight = Mathf.CeilToInt(affectedArea.height);
        float[,] heights = new float[width, heightMapHeight];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < heightMapHeight; z++)
            {
                heights[x, z] = height;
            }
        }

        data.SetHeights(Mathf.FloorToInt(affectedArea.xMin), Mathf.FloorToInt(affectedArea.yMin), heights);
    }
}
#endif

public class TerrainFlatten : MonoBehaviour
{
    public float flattenRadius = 5f;
    public bool applyOnRemoval = true;
}
