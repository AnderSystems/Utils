using System.Linq;
using UnityEngine;

public class CheatCode : MonoBehaviour
{
    public static string pressedKeys;
    public bool show;
    public static string lastCheatEnable;

    public void OnGUI()
    {
        //if (!Application.isEditor)
        //    return;
        if (show)
        {
            GUILayout.Label("<b>[Pressed keys]: </b>" + pressedKeys);
        }
    }

    public void HideCheatBox()
    {
        lastCheatEnable = "";
    }

    private void Start()
    {
        pressedKeys = "";
    }

    /// <summary>
    /// Check a code (put the keys separated by commas: ",")
    /// </summary>
    /// <param name="cheat"></param>
    /// <returns></returns>
    public static bool CheatCheck(string cheat)
    {
        bool r = false;
        //if (Application.isEditor)
        {
            if (!string.IsNullOrEmpty(pressedKeys))
            {
                if (pressedKeys.Contains(cheat.ToUpper()))
                {
                    r = true;
                    pressedKeys = "";
                    //Interface.PlayAudio("CheatActivated");
                    Debug.Log($"[Cheat Code] Cheat: '{cheat.ToUpper()}' activated!");
                    pressedKeys += "\n<b>CHEAT ACTIVATED!</b>\n";
                    //Console.Alert("Cheat Enabled!", $"Cheat: '{cheat.ToUpper()}' activated!", null, 5);
                    lastCheatEnable = cheat.ToUpper().Replace(",", "");
                }
            }
        }
        return r;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            show = !show;
        }

        KeyCode[] allKeys = System.Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
        foreach (var key in allKeys)
        {
            if (pressedKeys.Length >= 100)
            {
                pressedKeys = pressedKeys.Remove(0, 1);
            }
            if (Input.GetKeyDown(key))
            {
                pressedKeys += key + ",";
            }
        }

        if (CheatCheck("S,L,O,W,M,O"))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0.1f;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }
    private float fixedTimeStep;
    private void OnApplicationQuit()
    {
        pressedKeys = "";
    }
}
