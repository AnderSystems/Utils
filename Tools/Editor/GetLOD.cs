using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class GetLOD : MonoBehaviour
{
    [MenuItem("GameObject/Create LOD", true)]
    static bool ValidadeCreateLOD()
    {
        // Return false if no transform is selected.
        return Selection.activeGameObject.name.ToLower().Contains("_lod");
    }

    [MenuItem("GameObject/Create LOD", priority = 0)]
    static void CreateLOD()
    {
        GameObject s = Selection.activeGameObject;
        int index = s.transform.GetSiblingIndex();
        GameObject go = new GameObject(Regex.Split(s.name, "_lod", RegexOptions.IgnoreCase)[0]);
        List<Renderer> lods = new List<Renderer>();

        Undo.RegisterCompleteObjectUndo(go, $"Create LOD for {go.name}");

        foreach (var item in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (item.transform.parent == s.transform.parent)
            {
                if (item.name.ToLower().Contains(go.name.ToLower() + "_lod"))
                {
                    lods.Add(item);
                }
            }
        }

        // Ordena os LODs por nome
        lods = lods.OrderBy(x => x.name).ToList();

        go.transform.position = s.transform.position;
        go.transform.eulerAngles = new Vector3(0, s.transform.eulerAngles.y, 0);
        go.transform.parent = s.transform.parent;
        go.transform.SetSiblingIndex(index);

        LODGroup goLod = go.AddComponent<LODGroup>();
        goLod.fadeMode = LODFadeMode.CrossFade;

        List<LOD> ll = new List<LOD>();
        Dictionary<string, GameObject> groups = new Dictionary<string, GameObject>();
        List<Renderer> AllRenderers = new List<Renderer>();


        for (int i = 0; i < lods.Count; i++)
        {
            // Cria uma lista para guardar os Renderers do LOD atual
            List<Renderer> currentLODRenderers = new List<Renderer> { lods[i] };
            AllRenderers.Add(lods[i]);

            // Verifica se o LOD atual tem filhos com renderizadores
            Renderer[] childRenderers = lods[i].GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (!currentLODRenderers.Contains(childRenderer))
                {
                    AllRenderers.Add(childRenderer);
                    currentLODRenderers.Add(childRenderer);
                }
            }

            float ii = (100 - ((90 / lods.Count) * (i + 1))) / 100.0f;
            string name = AllRenderers[i].gameObject.name.Split('_')[0];

            // Cria o grupo para o objeto
            if (!groups.ContainsKey(name))
            {
                GameObject groupObject = new GameObject(name);

                groupObject.transform.position = AllRenderers[i].transform.position;
                groupObject.transform.rotation = AllRenderers[i].transform.rotation;
                groupObject.transform.parent = go.transform;

                groups.Add(name, groupObject);
            }

            LOD lod = new LOD(ii, currentLODRenderers.ToArray());
            lod.fadeTransitionWidth = (i == 0) ? 0.15f : 0.05f;
            ll.Add(lod);
        }

        for (int i = 0; i < AllRenderers.Count; i++)
        {
            string name = AllRenderers[i].gameObject.name.Split('_')[0];
            if (groups.ContainsKey(name))
            {
                groups[name].transform.parent = go.transform;
                AllRenderers[i].transform.parent = groups[name].transform;
                AllRenderers[i].transform.localPosition = Vector3.zero;
            }
        }

        lods.Reverse();

        goLod.SetLODs(ll.ToArray());
        goLod.RecalculateBounds();

        Selection.activeGameObject = go;
    }
}

