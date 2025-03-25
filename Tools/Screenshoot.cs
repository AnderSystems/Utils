using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows;

#region EditorScript
//Editor Script
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Screenshoot))]
public class ScreenshootEditor : Editor
{
    //Editor Target
    Screenshoot t;

    // Call Voids
    void OnSceneGUI()
    {
        SetTarget();
    }

    bool Fouldout;

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        SetTarget();
        if(GUILayout.Button("Take Screenshoot"))
        {
            TakeScreenShoot();
        }

        Fouldout = EditorGUILayout.Foldout(Fouldout, "Settings");

        if (Fouldout)
        {
            base.OnInspectorGUI();
        }
    }

    public void TakeScreenShoot()
    {
        SetTarget();

        int Result = EditorUtility.DisplayDialogComplex("Save Screenshot?", "Screenshot captured! Save or copy.", "Save", "Never Ask Again", "Cancel");

        t.LastSaveLocation = (EditorUtility.SaveFilePanel("Save Screensshot", Application.dataPath, t.LastScreenShootName, "png"));
        Screenshoot.TakeScreenShotAnSave(t.LastSaveLocation, t.Size);

        if (Result == 1)
        {
            t.AskToSave = false;
        }
    }

    //Set Target of this editor
    void SetTarget()
    {
        t = (Screenshoot)target;
    }
}

#endif
#endregion

public class Screenshoot : MonoBehaviour
{
    public string LastScreenShootName { get { return Path.GetFileName(LastSaveLocation); } }
    public string LastSaveLocation = "";
    public bool AskToSave = true;
    public int Size = 1;

    public static Texture2D TakeScreenShot(int Size)
    {
        Texture2D r = ScreenCapture.CaptureScreenshotAsTexture(Size);
        return r;
    }
    public static void TakeScreenShotAnSave(string SavePath, int Size)
    {
        //File.WriteAllBytes(SavePath, TakeScreenShot(Size).GetRawTextureData());
        ScreenCapture.CaptureScreenshot(SavePath, Size);
    }
}
