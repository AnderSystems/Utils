using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnnotationsEditor : EditorWindow, IHasCustomMenu
{
    private List<string> filePaths = new List<string>(); // Caminhos dos arquivos abertos
    private List<string> fileContents = new List<string>(); // Conteúdo dos arquivos
    private int currentTab = 0;
    private Vector2 scrollPosition;
    private const string prefsKey = "AnnotationsFilePaths";

    [MenuItem("Tools/Annotations")]
    public static void OpenWindow()
    {
        GetWindow<AnnotationsEditor>("Annotations");
    }

    private void OnEnable()
    {
        LoadProjectPreferences();
    }

    private void OnDisable()
    {
        SaveProjectPreferences();
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("File/Open"), false, OpenFile);
        menu.AddItem(new GUIContent("File/Save"), false, () => SaveFile(false));
        menu.AddItem(new GUIContent("File/Save As"), false, () => SaveFile(true));
        menu.AddItem(new GUIContent("File/Open in Notepad"), false, OpenInNotepad);
        menu.AddItem(new GUIContent("File/Reveal in Explorer"), false, RevealInExplorer);
    }

    private void OnGUI()
    {
        if (filePaths.Count == 0)
        {
            if (GUILayout.Button("Open File"))
                OpenFile();
            return;
        }

        currentTab = GUILayout.Toolbar(currentTab, filePaths.ConvertAll(Path.GetFileName).ToArray());

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("📄", GUILayout.Width(32))) SaveFile(true);
        if (GUILayout.Button("📁", GUILayout.Width(32))) OpenFile();
        if (GUILayout.Button("💾", GUILayout.Width(32))) SaveFile(false);
        if (GUILayout.Button("❌", GUILayout.Width(32))) CloseCurrentTab();
        GUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        fileContents[currentTab] = EditorGUILayout.TextArea(fileContents[currentTab], GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void OpenFile()
    {
        string path = EditorUtility.OpenFilePanel("Open Text File", GetDirectoryFromPath(), "txt");
        if (string.IsNullOrEmpty(path)) return;

        if (!filePaths.Contains(path))
        {
            filePaths.Add(path);
            fileContents.Add(File.ReadAllText(path));
            currentTab = filePaths.Count - 1;
        }
    }

    private void SaveFile(bool saveAs)
    {
        if (saveAs || string.IsNullOrEmpty(filePaths[currentTab]))
        {
            string path = EditorUtility.SaveFilePanel("Save Text File", GetDirectoryFromPath(), "NewFile.txt", "txt");
            if (string.IsNullOrEmpty(path)) return;
            filePaths[currentTab] = path;
        }

        File.WriteAllText(filePaths[currentTab], fileContents[currentTab]);
        SaveProjectPreferences();
        Debug.Log($"Annotation saved on {filePaths[currentTab]}");
    }

    private void CloseCurrentTab()
    {
        if (filePaths.Count > 0)
        {
            filePaths.RemoveAt(currentTab);
            fileContents.RemoveAt(currentTab);
            currentTab = Mathf.Clamp(currentTab, 0, filePaths.Count - 1);
        }
    }

    private void OpenInNotepad()
    {
        if (!string.IsNullOrEmpty(filePaths[currentTab]))
            System.Diagnostics.Process.Start("notepad.exe", filePaths[currentTab]);
    }

    private void RevealInExplorer()
    {
        if (!string.IsNullOrEmpty(filePaths[currentTab]))
            System.Diagnostics.Process.Start("explorer.exe", GetDirectoryFromPath());
    }

    private string GetDirectoryFromPath()
    {
        return filePaths.Count > 0 && !string.IsNullOrEmpty(filePaths[currentTab])
            ? Path.GetDirectoryName(filePaths[currentTab])
            : Application.dataPath;
    }

    private void LoadProjectPreferences()
    {
        filePaths = new List<string>(EditorPrefs.GetString(prefsKey, "").Split('|'));
        fileContents.Clear();

        for (int i = 0; i < filePaths.Count; i++)
        {
            if (File.Exists(filePaths[i]))
                fileContents.Add(File.ReadAllText(filePaths[i]));
            else
                fileContents.Add("");
        }
    }

    private void SaveProjectPreferences()
    {
        EditorPrefs.SetString(prefsKey, string.Join("|", filePaths));
    }
}
