using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;

[CustomEditor(typeof(ShortcutFile))]
public class ShortcutFile_Editor : Editor
{
    private static Texture2D _shortcutIcon;

    private static Texture2D ShortcutIcon
    {
        get
        {
            if (_shortcutIcon == null)
            {
                _shortcutIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/ShortcutFileIcon.png");
            }
            return _shortcutIcon;
        }
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        ShortcutFile t = EditorUtility.InstanceIDToObject(instanceID) as ShortcutFile;
        if (t != null)
        {
            OpenShortcutFile(t.shortcutPath);
            return true;
        }
        return false;
    }

    private ShortcutFile t { get { return (ShortcutFile)target; } }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Shortcut Path");
        EditorGUILayout.BeginHorizontal();
        t.shortcutPath = GUILayout.TextField(t.shortcutPath);
        if (GUILayout.Button("...", GUILayout.Width(24)))
        {
            t.SelectPath = true;
        }
        EditorGUILayout.EndHorizontal();

        string objPath = AssetDatabase.GetAssetPath(t);

        if (string.IsNullOrEmpty(objPath))
            return;

        if (t.SelectPath)
        {
            // Define o caminho inicial: `shortcutPath` ou onde o objeto está salvo.
            string initialPath = string.IsNullOrEmpty(t.shortcutPath)
                ? System.IO.Path.GetDirectoryName(objPath)
                : t.shortcutPath;

            t.shortcutPath = EditorUtility.OpenFilePanel("Select Shortcut", initialPath, "");
            if (!string.IsNullOrEmpty(t.shortcutPath))
            {
                t.SelectPath = false;
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                UpdateIcon(t); // Atualiza o ícone
            }
        }

        if (!string.IsNullOrEmpty(t.shortcutPath))
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(t.shortcutPath);
            AssetDatabase.RenameAsset(objPath, name);
        }

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        // Botão para abrir o caminho do atalho
        GUI.enabled = CheckFile(t.shortcutPath);
        if (GUILayout.Button("Ping"))
        {
            PingFile(t.shortcutPath);
        }
        GUI.enabled = true;

        // Botão pingar o objeto
        if (GUILayout.Button("Open"))
        {
            OpenShortcutFile(t.shortcutPath);
        }

        // Botão para voltar ao objeto do atalho
        if (GUILayout.Button("Back"))
        {
            Selection.activeObject = t;
            EditorGUIUtility.PingObject(t);
        }
        GUILayout.EndHorizontal();
    }

    private static void UpdateIcon(ShortcutFile shortcut)
    {
        if (ShortcutIcon != null)
        {
            EditorGUIUtility.SetIconForObject(shortcut, ShortcutIcon);
        }
    }

    public static bool CheckFile(string path)
    {
        // Verifica se a pasta está na Unity (Assets)
        if (path.StartsWith(Application.dataPath))
        {
            string unityPath = path.Replace(Application.dataPath, "Assets");
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(unityPath, typeof(UnityEngine.Object));
            if (obj != null)
            {
                return true;
            }
        }
        return false;
    }

    private static void PingFile(string path)
    {
        // Verifica se a pasta está na Unity (Assets)
        if (path.StartsWith(Application.dataPath))
        {
            string unityPath = path.Replace(Application.dataPath, "Assets");
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(unityPath, typeof(UnityEngine.Object));
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                return;
            }
        }
    }

    private static void OpenShortcutFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Error", "Shortcut path is empty.", "OK");
            return;
        }

        // Caso contrário, tenta abrir no sistema operacional
        if (System.IO.File.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "The file does not exist or is invalid.", "OK");
        }
    }
}

#endif

[CreateAssetMenu(menuName = "Shortcut/File")]
public class ShortcutFile : ScriptableObject
{
    public string shortcutPath;
    public bool SelectPath = true;
}