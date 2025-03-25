using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class DocumentationGenerator : Editor
{
    [MenuItem("Assets/Generate Documentation")]
    public static void GenerateDocumentation()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        //Generate(AssetDatabase.GetAssetPath(Selection.activeObject));
        SaveFile(path, GenerateHTML(path));
    }
    public struct docData
    {
        public string summary;
        public string methodOrVar;
    }

    public static string GenerateText(string path)
    {
        string pathXML = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".txt";
        string text = File.ReadAllText(path);

        string result = "";
        docData methodData = new docData();
        docData varData = new docData();

        List<docData> methodsData = new List<docData>();
        List<docData> varsData = new List<docData>();

        foreach (var line in text.Split('\n'))
        {
            string l = line;
            if (line.Split("\">").Length > 1)
            {
                l = line.Split("\">")[1];
            }
            l = line.Replace("</param>", "");
            l = line.Replace("</remarks>", "");

            if (l.Contains("[Tooltip("))
            {
                string summary = l.Split("\"")[1];
                varData.summary = summary;
            }
            else
            {
                if (string.IsNullOrEmpty(varData.methodOrVar) && !string.IsNullOrEmpty(varData.summary))
                {
                    varData.methodOrVar = l;
                    varsData.Add(new docData() { summary = varData.summary, methodOrVar = varData.methodOrVar });
                    varData = new docData();
                }
            }

            if (!string.IsNullOrEmpty(varData.methodOrVar))
                continue;
            //Methods and summary
            if (!l.Contains("/>") && !l.Contains("["))
            {
                if (l.Contains("///"))
                {
                    if (!l.Contains("summary>"))
                    {
                        methodData.summary = l;
                    }
                }
                else
                {
                    if (!l.Contains(": "))
                    {
                        if (!string.IsNullOrEmpty(methodData.summary))
                        {
                            methodData.methodOrVar = l;
                        }
                    }
                }
            }

            if (methodData.methodOrVar != null && methodData.summary != null)
            {
                //Debug.Log("docData.summary: " + docData.summary + " | docData.methodOrVar: " + docData.methodOrVar);
                methodsData.Add(new docData() { summary = methodData.summary, methodOrVar = methodData.methodOrVar });
                methodData.summary = null;
                methodData.methodOrVar = null;
            }
        }

        //if (methodsData.Count > 0)
        //{
        //    result += "<h3>METHODS</h3>\n\n";
        //}
        // Methods
        foreach (var item in methodsData)
        {
            string method = item.methodOrVar;
            string summary = item.summary;
            summary = summary.Replace("</param>", "");
            summary = summary.Replace("</remarks>", "");
            summary = summary.Replace("\n", "");
            method = method.Replace("\n", "");

            method = method.TrimStart();
            summary = summary.TrimStart();

            if (summary.Contains(">"))
            {
                string removeWord = summary.Split('<')[1];
                removeWord = summary.Split('>')[0];
                summary = summary.Replace(removeWord, "");
                summary = summary.Replace(">", "");
            }


            string template = "<div class=\"notranslate method\">METHOD</div><p class=\"method-desc\">DESC</p>";

            if (method.Trim() == "{" || summary.Trim() == "{")
            {
                //result += "<h2>\n" + summary.Replace("\n", "") + "</h2>\n\n";
                result += "<h2>" + summary.Replace("\n", "") + "</h2>\n\n";
            }
            else
            {
                if (!result.Contains("<h3>METHODS</h3>\n\n"))
                {
                    result += "<h3>METHODS</h3>\n\n";
                }
                result += template.Replace("METHOD", method.Replace("\n", "")).Replace("DESC", summary.Replace("\n", ""));
            }


        }

        //Vars
        if (varsData.Count > 0)
        {
            result += "<h3>VARIABLES</h3>\n\n";
        }
        foreach (var item in varsData)
        {
            string var = item.methodOrVar;
            string summary = item.summary;

            string template = "<div class=\"notranslate variable-desc\"><p class=\"variable-title\">VARIABLE</p>DESC</div>";

            result += template.Replace("VARIABLE", var).Replace("DESC", summary) + "\n\n";
        }

        result = result.Replace("///", "");
        return result;
        //Debug.Log(result);
        //File.CreateText(pathXML).Close();
        //File.WriteAllText(pathXML, result);
    }

    public static string GenerateHTML(string path)
    {
        string pathXML = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".txt";
        string fileTxt = File.ReadAllText(path);

        string title = Path.GetFileNameWithoutExtension(path);


        string text = GenerateText(path);

        string result = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n<title>Page Title</title>\r\n<style>* {font-family: sans-serif;}\r\nbody {background-color: #242424;\r\nmargin-left: 20%;\r\nmargin-right: 20%;\r\ncolor: #cccccc;\r\n}\r\nh1 {text-align: left;\r\ncolor: white;}\r\nh2 {text-align: left;\r\nmargin-top: -20px;\r\nmargin-bottom: 60px;\r\nfont-weight: normal;\r\ncolor: #cccccc;\r\n}\r\n.method { border: 1px solid;\r\n  \tborder-radius: 5px;\r\n\tpadding: 10px;\r\n\tmargin: 10px 0;\r\n    margin-bottom: -10px;\r\n    background-color: #333333;\r\n    border-color: #b0b0b0;\r\n    color: white;\r\n}\r\n\r\n.method-desc {\r\n    margin-bottom: 32px;\r\n    margin-left: 6px;\r\n}\r\n\r\ndiv { margin-top: 20px;\r\ncolor: #cccccc;\r\n}\r\n.variable-title{\r\ncolor: white;\r\nmargin-bottom: 0px;\r\n}\r\n\r\n.variable-desc {\r\ncolor: #cccccc;\r\nmargin-bottom: 20px;\r\n}\r\n\r\n</style>\r\n</head>\r\n<body>\r\n\r\n<h1>CodeName</h1>\r\nCODE HERE\r\n\r\n</body>\r\n</html>";

        result = result.Replace("Page Title", title);
        result = result.Replace("CodeName", title);
        result = result.Replace("CODE HERE", text);

        return result;
    }

    public static void SaveFile(string path, string text)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path) + "/Documentation/"))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) + "/Documentation/");
        }
        string pathXML = Path.GetDirectoryName(path) + "/Documentation/" + Path.GetFileNameWithoutExtension(path) + ".html";
        File.CreateText(pathXML).Close();
        File.WriteAllText(pathXML, text);
    }
}
