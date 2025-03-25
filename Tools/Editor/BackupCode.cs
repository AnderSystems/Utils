using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BackupCode : Editor
{
    [MenuItem("Assets/Backup", priority = 99)]
    public static void Backup()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string dir = Path.GetDirectoryName(path) + "/~Backups~";

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.Copy(path, dir + "/" + Path.GetFileName(path), true);
    }

    [MenuItem("Assets/View Backups", priority = 100)]
    public static void ViewBackup()
    {
        string dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject)) + "/~Backups~";
        if (Directory.Exists(dir))
        {
            EditorUtility.RevealInFinder(dir + "/");
        } else
        {
            EditorUtility.DisplayDialog("Backups not found", "Backups not exists on folder", "Close");
        }
    }
}
