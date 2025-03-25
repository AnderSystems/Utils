using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshCombinerEditor : Editor
{
    static GameObject lastObjectOperation = null;

    class LODInfo
    {
        public int LODLevel;
        public List<GameObject> Objects = new List<GameObject>();

        public GameObject CombineMeshes()
        {
            List<Renderer> renderers = new List<Renderer>();
            Vector3 center = Vector3.zero;

            // Calcula o centro de todos os objetos
            foreach (var obj in Objects)
            {
                renderers.Add(obj.GetComponent<Renderer>());
                center += obj.transform.position;
            }

            center /= Objects.Count;

            // Cria listas de MeshFilters e materiais
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            List<Material> allMaterials = new List<Material>();
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            foreach (var renderer in renderers)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                // Adiciona os materiais
                allMaterials.AddRange(renderer.sharedMaterials);

                // Para cada submesh, cria um CombineInstance
                Mesh mesh = meshFilter.sharedMesh;
                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
                {
                    CombineInstance combineInstance = new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = submeshIndex,
                        transform = Matrix4x4.TRS(
                            meshFilter.transform.position - center,
                            meshFilter.transform.rotation,
                            meshFilter.transform.lossyScale
                        )
                    };

                    combineInstances.Add(combineInstance);
                }
            }

            // Cria o GameObject combinado
            GameObject combinedObject = new GameObject($"Combined_LOD_{LODLevel}");
            combinedObject.transform.position = center;

            Mesh combinedMesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // Suporte para meshes grandes
            };
            combinedMesh.CombineMeshes(combineInstances.ToArray(), false, true); // False para manter submeshes separados

            // Adiciona os componentes de Mesh
            MeshFilter meshFilterComponent = combinedObject.AddComponent<MeshFilter>();
            meshFilterComponent.mesh = combinedMesh;

            MeshRenderer meshRendererComponent = combinedObject.AddComponent<MeshRenderer>();
            meshRendererComponent.sharedMaterials = allMaterials.ToArray();

            return combinedObject;
        }
    }

    static List<LODInfo> GetLODs(GameObject[] selectedObjects)
    {
        List<LODInfo> lods = new List<LODInfo>();

        foreach (var obj in selectedObjects)
        {
            string objName = obj.name.ToLower();
            for (int i = 0; i < 10; i++)
            {
                LODInfo lod = null;
                foreach (var existingLOD in lods)
                {
                    if (existingLOD.LODLevel == i)
                    {
                        lod = existingLOD;
                        break;
                    }
                }

                if (objName.Contains(".lod" + i) || objName.Contains("_lod" + i))
                {
                    if (lod == null)
                    {
                        lod = new LODInfo { LODLevel = i };
                        lod.Objects = new List<GameObject> { obj };
                        lods.Add(lod);
                    }
                    else
                    {
                        lod.Objects.Add(obj);
                    }
                    break;
                }
            }
        }

        return lods;
    }

    [MenuItem("GameObject/Combine Meshes with LOD", priority = 1)]
    private static void CombineMeshesMenuOption()
    {
        if (Selection.activeGameObject != lastObjectOperation)
        {
            lastObjectOperation = null;
        }

        if (lastObjectOperation != null)
            return;
        List<GameObject> allObjects = new List<GameObject>();

        foreach (var selectedObject in Selection.gameObjects)
        {
            if (!allObjects.Contains(selectedObject))
            {
                allObjects.Add(selectedObject);
            }

            Renderer[] childRenderers = selectedObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in childRenderers)
            {
                if (!allObjects.Contains(renderer.gameObject))
                {
                    allObjects.Add(renderer.gameObject);
                }
            }
        }

        List<LODInfo> lods = GetLODs(allObjects.ToArray());

        GameObject parentObject = new GameObject(Selection.activeGameObject.name + "_Combined");
        LODGroup lodGroup = parentObject.AddComponent<LODGroup>(); // Adiciona o componente LOD
        lodGroup.fadeMode = LODFadeMode.CrossFade;
        lodGroup.animateCrossFading = true;
        List<LOD> lodGroupList = new List<LOD>();

        string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path; // Caminho da cena atual
        string sceneName = Path.GetFileNameWithoutExtension(scenePath); // Nome da cena
        string combinedMeshFolderPath = Path.Combine(Path.GetDirectoryName(scenePath), sceneName, "CombinedMeshes");

        if (!Directory.Exists(combinedMeshFolderPath))
        {
            Directory.CreateDirectory(combinedMeshFolderPath); // Cria a pasta se não existir
        }

        Undo.RegisterCreatedObjectUndo(parentObject, "Combine Meshes with LOD");

        foreach (var lod in lods)
        {
            LOD lodData = new LOD();

            GameObject combinedLODObject = lod.CombineMeshes();
            if (parentObject.transform.position == Vector3.zero)
            {
                parentObject.transform.rotation = combinedLODObject.transform.rotation;
                parentObject.transform.position = combinedLODObject.transform.position;
            }

            combinedLODObject.transform.parent = parentObject.transform;

            float lodIndex = lods.IndexOf(lod) + 1.0f;
            lodData.renderers = new Renderer[] { combinedLODObject.GetComponent<Renderer>() };
            lodData.screenRelativeTransitionHeight = (0.85f / lodIndex);
            if (lodGroupList.Count <= 0)
            {
                lodData.fadeTransitionWidth = 0.15f;
            }
            else
            {
                lodData.fadeTransitionWidth = 0.05f;
            }


            // Salva a mesh gerada no disco
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmssff");
            string meshName = $"{combinedLODObject.name}_{timestamp}_combined";
            string meshPath = Path.Combine(combinedMeshFolderPath, meshName + ".asset");

            AssetDatabase.CreateAsset(combinedLODObject.GetComponent<MeshFilter>().sharedMesh, meshPath); // Salva a mesh no caminho especificado
            AssetDatabase.SaveAssets(); // Salva os ativos

            lodGroupList.Add(lodData);
        }

        lodGroup.SetLODs(lodGroupList.ToArray());

        parentObject.transform.parent = Selection.activeGameObject.transform.parent;
        parentObject.transform.SetSiblingIndex(Selection.activeGameObject.transform.GetSiblingIndex());

        foreach (var item in Selection.transforms)
        {
            item.transform.parent = parentObject.transform;
            item.gameObject.SetActive(false);
        }

        Selection.activeGameObject = parentObject;
        lastObjectOperation = parentObject;
    }
}
