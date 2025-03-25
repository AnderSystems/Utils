using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GUIConsole : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F1;


    [System.Serializable]
    public enum LogPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    [SerializeField]
    public LogPosition logPosition = LogPosition.TopLeft;
    [Space]
    public float defaultLogTimeOut = 5;
    //[Range(0, 100)]
    //public float MaxWidth;
    public Color backgroundColor = new Color(0, 0, 0, 0.2f);

    [Range(0, 2)]
    public int expanded = 1;
    [System.Serializable]
    public enum rememberLastChoice
    {
        NotRemember,
        OnEditor,
        Always
    }
    [SerializeField]
    public rememberLastChoice lastChoice = rememberLastChoice.OnEditor;

    public void Notify(string logString, string stackTrace, LogType type)
    {
        Log log = GetLogByStackTrace(stackTrace);
        if (log == null)
        {
            if (logs.Count > 12)
            {
                logs.RemoveAt(0);
            }

            log = new Log();
            logs.Add(log);
        }

        log.logString = logString;
        log.stackTrace = stackTrace;
        log.type = type;
        log.timeOut = defaultLogTimeOut;
        log.numOfCalls += 1;
    }

    public Log GetLogByStackTrace(string stackTrace)
    {
        foreach (Log log in logs)
        {
            if (log.stackTrace == stackTrace)
            {
                return log;
            }
        }
        return null;
    }

    [System.Serializable]
    public class Log
    {
        public string logString;
        public string stackTrace;
        public LogType type;
        public int numOfCalls;
        public float timeOut;
    }
    [SerializeField]
    public List<Log> logs = new List<Log>();

    public void LastUpdate()
    {
        for (int i = logs.Count - 1; i >= 0; i--)
        {
            logs[i].timeOut -= 0.1f;
            if (logs[i].timeOut <= 0)
            {
                logs.RemoveAt(i);
            }
        }
    }

    //Mono
    void Start()
    {
        Application.logMessageReceived += Notify;
        InvokeRepeating("LastUpdate", 0, .1f);
    }
    public void OnGUI()
    {
        if (expanded == 0)
            return;

        string logString = "";

        foreach (Log log in logs)
        {
            string numOfCalls = "";

            if (expanded == 1)
            {
                if (log.numOfCalls < 99)
                {
                    numOfCalls = log.numOfCalls.ToString();
                }
                else
                {
                    numOfCalls = "+99";
                }
            }
            else
            {
                if (log.numOfCalls < 9999)
                {
                    numOfCalls = log.numOfCalls.ToString();
                }
                else
                {
                    numOfCalls = "+9999";
                }
            }

            CodeInfo codeInfo = ExtractCodeInfo(log.stackTrace);

            string prefix = "ℹ️ " + numOfCalls + ".";
            string suffix = "";

            if (log.type == LogType.Error || log.type == LogType.Exception)
            {
                prefix = "⛔ <color=red>" + numOfCalls + ".";
                suffix = "</color>";
            }

            if (log.type == LogType.Warning)
            {
                prefix = "⚠️ <color=yellow>" + numOfCalls + ".";
                suffix = "</color>";
            }

            prefix += $"<b>[{codeInfo.CodeName}] </b>";

            if (expanded == 1)
            {
                logString += $"\n{prefix}{log.logString}\n{ExtractLineInfo(log.stackTrace)}{suffix}\n";
            }

            if (expanded == 2)
            {
                logString += $"\n{prefix}{log.logString}\n{HighlightStackTrace(log.stackTrace, true)}{suffix}\n";
            }
        }

        GUIStyle shadowStyle = new GUIStyle();
        shadowStyle.normal.textColor = new Color(0, 0, 0, 0.5f);
        string nonColor = Regex.Replace(logString, @"<color=[^>]+>|</color>", string.Empty, RegexOptions.IgnoreCase);

        GUI.Label(new Rect(10, 11, Screen.width, Screen.height), nonColor, shadowStyle);
        GUI.Label(new Rect(11, 10, Screen.width, Screen.height), nonColor, shadowStyle);
        GUI.Label(new Rect(9, 10, Screen.width, Screen.height), nonColor, shadowStyle);
        GUI.Label(new Rect(10, 9, Screen.width, Screen.height), nonColor, shadowStyle);



        GUIStyle defaultStyle = new GUIStyle();
        defaultStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, Screen.width, Screen.height), logString, defaultStyle);
    }

    // Função para destacar a linha do stack trace
    private string HighlightStackTrace(string stackTrace, bool bold)
    {
        string highlightedStackTrace = stackTrace;
        string pattern = @"\(at\s.+\.cs:\d+\)"; // Padrão para "(at arquivo.cs:linha)"
        Regex regex = new(pattern);

        highlightedStackTrace = regex.Replace(stackTrace, match =>
        {
            if (bold)
            {
                return $"<u>{match.Value}</u>"; // Adiciona sublinhado
            }
            else
            {
                return $"<b><u>{match.Value}</u></b>"; // Adiciona negrito e sublinhado
            }

        });

        return highlightedStackTrace;
    }

    // Função para extrair a linha do stack trace
    private string ExtractLineInfo(string stackTrace)
    {
        string pattern = @"\(at\s.+\.cs:\d+\)"; // Padrão para "(at arquivo.cs:linha)"
        Regex regex = new Regex(pattern);
        Match match = regex.Match(stackTrace);

        if (match.Success)
        {
            return $"<u>{match.Value}</u>"; // Retorna somente a linha do código
        }
        return "No line info"; // Caso não encontre a linha
    }

    public struct CodeInfo
    {
        public string CodeName;
        public int Line;
        public string FunctionName;
    }

    public CodeInfo ExtractCodeInfo(string stackTrace)
    {
        string pattern = @"\(at\s(.+)\.cs:(\d+)\)"; // Padrão para "(at arquivo.cs:linha)"
        Regex regex = new Regex(pattern);
        Match match = regex.Match(stackTrace);

        if (match.Success)
        {
            string fullPath = match.Groups[1].Value;
            string codeName = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            int line = int.Parse(match.Groups[2].Value);
            string functionName = ExtractFunctionName(stackTrace);
            return new CodeInfo { CodeName = codeName, Line = line, FunctionName = functionName };
        }

        return new CodeInfo { CodeName = "No code info", Line = -1, FunctionName = "No function info" }; // Caso não encontre a linha
    }

    private string ExtractFunctionName(string stackTrace)
    {
        string pattern = @"at\s(.+)\s\(";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(stackTrace);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return "No function info";
    }

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("ConsoleExpanded"))
        {
            expanded = PlayerPrefs.GetInt("ConsoleExpanded");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            expanded = (int)(Mathf.Repeat(expanded + 1, 3));

            if (lastChoice == rememberLastChoice.Always)
            {
                PlayerPrefs.SetInt("ConsoleExpanded", expanded);
            }
            else
            {
                if (lastChoice == rememberLastChoice.OnEditor)
                {
                    if (Application.isEditor)
                    {
                        PlayerPrefs.SetInt("ConsoleExpanded", expanded);
                    }
                }
            }
        }
    }

    // Método para criar uma textura 1x1 com a cor desejada
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}