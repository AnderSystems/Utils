using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    [MenuItem("File/Open Build Location")]
    public static void OpenBuildLocation()
    {
        string buildPath = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);

        if (!string.IsNullOrEmpty(buildPath) && Directory.Exists(Path.GetDirectoryName(buildPath)))
        {
            Process.Start(Path.GetDirectoryName(buildPath));
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "No valid build path found. Please make a build first.", "OK");
        }
    }

    [MenuItem("File/Open Build")]
    public static void OpenBuild()
    {
        string buildPath = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);



        if (string.IsNullOrEmpty(buildPath) || !File.Exists(buildPath))
        {
            buildPath += "/index.html";
        }

        if (File.Exists(buildPath))
        {
            OpenWebGLBuild(buildPath);
            return;
        }

        if (string.IsNullOrEmpty(buildPath) || !File.Exists(buildPath))
        {
            EditorUtility.DisplayDialog("Error", "No valid build executable found. Please make a build first.", "OK");
            return;
        }

        string extension = Path.GetExtension(buildPath).ToLower();
        if (extension == ".exe")
        {
            Process.Start(buildPath);
        }
        else if (extension == ".html" || extension == ".htm")
        {
            Process.Start("cmd.exe", $"cd {buildPath}");
        }
        else if (extension == ".apk")
        {
            if (IsAndroidDeviceConnected())
            {
                string adbPath = GetAdbPath();
                Process.Start(adbPath, $"install -r \"{buildPath}\"");
                EditorUtility.DisplayDialog("Success", "Build installed on the connected Android device.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "No Android device connected.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Unsupported build format.", "OK");
        }
    }

    static void OpenWebGLBuild(string buildPath)
    {
        UnityEngine.Debug.Log($"Open server on path {buildPath}");
        // Extrai o diretório onde o index.html está
        string buildDirectory = Path.GetDirectoryName(buildPath);
        // Obtém o diretório pai (a "pasta da pasta")
        string parentFolder = Directory.GetParent(buildDirectory)?.FullName;

        if (parentFolder != null)
        {
            // Caminho completo para o arquivo index.js na pasta da pasta
            string indexJsPath = Path.Combine(parentFolder, "index.js");

            if (File.Exists(indexJsPath))
            {
                // Se index.js existe, cria e executa um arquivo batch para rodar o script Node.js
                string batchContent = $"@echo off\ncd /d \"{parentFolder}\"\nnode index.js\npause";
                string batchFilePath = Path.Combine(buildDirectory, "run_node.bat");

                File.WriteAllText(batchFilePath, batchContent);

                // Executa o arquivo batch
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchFilePath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Caso contrário, abre um servidor local apontando para a build
                StartLocalServer(buildDirectory);
            }
        }
    }

    static void StartLocalServer(string buildDirectory)
    {
        // Exemplo básico: iniciar um servidor local usando Python (certifique-se de que está instalado)
        string pythonServerCommand = "python -m http.server 8000";

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c cd /d \"{buildDirectory}\" && {pythonServerCommand}",
            UseShellExecute = false
        });
    }


    private static bool IsAndroidDeviceConnected()
    {
        string adbPath = GetAdbPath();
        Process adbProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = "devices",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        adbProcess.Start();
        string output = adbProcess.StandardOutput.ReadToEnd();
        adbProcess.WaitForExit();

        return output.Contains("\tdevice");
    }

    private static string GetAdbPath()
    {
        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        return Path.Combine(sdkPath, "platform-tools", "adb");
    }
}
