using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;

[CustomEditor(typeof(ShortcutFolder))]
public class ShortcutFolder_Editor : Editor
{
    private static Texture2D _shortcutIcon;

    private static Texture2D ShortcutIcon
    {
        get
        {
            if (_shortcutIcon == null)
            {
                _shortcutIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/ShortcutFolderIcon.png");
            }
            return _shortcutIcon;
        }
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        ShortcutFolder t = EditorUtility.InstanceIDToObject(instanceID) as ShortcutFolder;
        if (t != null)
        {
            OpenShortcutFolder(t.shortcutPath);
            return true;
        }
        return false;
    }

    private ShortcutFolder t { get { return (ShortcutFolder)target; } }

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

            t.shortcutPath = EditorUtility.OpenFolderPanel("Select Shortcut", initialPath, "");
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

        // Botão para abrir o caminho do atalho
        if (GUILayout.Button("Go to Folder"))
        {
            OpenShortcutFolder(t.shortcutPath);
        }

        // Botão para voltar ao objeto do atalho
        if (GUILayout.Button("Back to Shortcut Object"))
        {
            Selection.activeObject = t;
            EditorGUIUtility.PingObject(t);
        }
    }

    private static void UpdateIcon(ShortcutFolder shortcut)
    {
        if (ShortcutIcon != null)
        {
            EditorGUIUtility.SetIconForObject(shortcut, ShortcutIcon);
        }
    }

    private static void OpenShortcutFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Error", "Shortcut path is empty.", "OK");
            return;
        }

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

        // Caso contrário, tenta abrir no sistema operacional
        if (System.IO.Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "The folder does not exist or is invalid.", "OK");
        }
    }
}

#endif

[CreateAssetMenu(menuName = "Shortcut/Folder")]
public class ShortcutFolder : ScriptableObject
{
    public string shortcutPath;
    public bool SelectPath = true;
}
