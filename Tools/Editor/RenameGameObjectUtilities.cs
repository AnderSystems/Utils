using System.Linq;
using UnityEditor;
using UnityEngine;

public class RenameGameObjectUtilities : EditorWindow
{
    private static string searchText = "";
    private static string replaceText = "";

    [MenuItem("GameObject/Name/Organize", false, 0)]
    private static void Organize()
    {
        var selection = Selection.gameObjects;
        var sorted = selection.OrderBy(go => go.name, System.StringComparer.OrdinalIgnoreCase).ToArray();
        RenameInOrder(sorted);
    }

    [MenuItem("GameObject/Name/Organize Inverted", false, 1)]
    private static void OrganizeInverted()
    {
        var selection = Selection.gameObjects;
        var sorted = selection.OrderByDescending(go => go.name, System.StringComparer.OrdinalIgnoreCase).ToArray();
        RenameInOrder(sorted);
    }

    [MenuItem("GameObject/Name/Remove Duplicate Names", false, 2)]
    private static void RemoveDuplicateNames()
    {
        var selection = Selection.gameObjects;
        var nameCounts = selection.GroupBy(go => go.name).ToDictionary(g => g.Key, g => 0);

        foreach (var go in selection)
        {
            var baseName = go.name;
            if (nameCounts[baseName] > 0)
            {
                go.name = $"{baseName}.{nameCounts[baseName]}";
            }
            nameCounts[baseName]++;
        }
    }

    [MenuItem("GameObject/Name/Replace Text", false, 3)]
    private static void ShowReplaceWindow()
    {
        ShowWindow("Replace Text");
    }

    private static void ShowWindow(string title)
    {
        var window = GetWindow<RenameGameObjectUtilities>(true, title);
        window.minSize = new Vector2(300, 150);
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Text in Names", EditorStyles.boldLabel);
        searchText = EditorGUILayout.TextField("Search:", searchText);
        replaceText = EditorGUILayout.TextField("Replace with:", replaceText);

        if (GUILayout.Button("Apply"))
        {
            ApplyReplaceText();
            Close();
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    private static void ApplyReplaceText()
    {
        if (string.IsNullOrEmpty(searchText))
        {
            Debug.LogWarning("Search text cannot be empty.");
            return;
        }

        var selection = Selection.gameObjects;
        foreach (var go in selection)
        {
            go.name = go.name.Replace(searchText, replaceText);
            go.name = go.name.Replace("<i>", go.transform.GetSiblingIndex().ToString());
            go.name = go.name.Replace("<index>", go.transform.GetSiblingIndex().ToString());
        }
    }

    private static void RenameInOrder(GameObject[] sorted)
    {
        for (int i = 0; i < sorted.Length; i++)
        {
            sorted[i].name = i.ToString();
        }
    }
}
