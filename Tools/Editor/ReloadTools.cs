using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class ReloadTools
{
    static ReloadTools()
    {
        if (IsReloadDomainOnExitPlay)
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
    }

    private const string MenuReloadDomainOnExit = "Tools/Reload/Reload Domain On Exit Play";
    private const string SettingName = "ReloadDomainOnExitPlay"; // Define o nome da chave para salvar a configuração

    // Adiciona "Reload Scene" no menu Tools/Reload
    [MenuItem("Tools/Reload/Reload Scene")]
    public static void ReloadScene()
    {
        // Salva a cena atual e recarrega
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
    }

    // Adiciona "Reload Domain" no menu Tools/Reload
    [MenuItem("Tools/Reload/Reload Domain")]
    public static void ReloadDomain()
    {
        // Faz o reload do domínio (usado para recarregar os scripts)
        EditorUtility.RequestScriptReload();
        AssetDatabase.Refresh();
        Debug.Log("Domain reloaded.");
    }

    // Adiciona "Reload Domain and Scene" no menu Tools/Reload/Reload Domain and Scene"
    [MenuItem("Tools/Reload/Reload Domain and Scene")]
    public static void ReloadDomainAndScene()
    {
        ReloadDomain();
        ReloadScene();
    }

    // Propriedade que armazena o estado do toggle
    private static bool IsReloadDomainOnExitPlay
    {
        get { return EditorPrefs.GetBool(SettingName, false); }
        set { EditorPrefs.SetBool(SettingName, value); }
    }

    // Adiciona o toggle "Reload Domain On Exit Play" no menu Tools/Reload
    [MenuItem(MenuReloadDomainOnExit)]
    private static void ToggleReloadDomainOnExitPlay()
    {
        // Alterna o estado da opção
        IsReloadDomainOnExitPlay = !IsReloadDomainOnExitPlay;
        Debug.Log($"Reload Domain On Exit Play: {IsReloadDomainOnExitPlay}");

        // Se a opção foi ativada, registra o evento para recarregar o domínio ao sair do PlayMode
        if (IsReloadDomainOnExitPlay)
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        else
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }


    // Valida o estado do toggle (se está marcado ou não)
    [MenuItem(MenuReloadDomainOnExit, true)]
    private static bool ValidateToggleReloadDomainOnExitPlay()
    {
        UnityEditor.Menu.SetChecked(MenuReloadDomainOnExit, IsReloadDomainOnExitPlay);
        return true;
    }

    // Verifica se a opção está ativada e recarrega o domínio ao sair do PlayMode
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ReloadDomain();
            Debug.Log("Domain reloaded on exit PlayMode.");
        }
    }
}
