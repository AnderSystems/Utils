using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SelectionHistoryWindow : EditorWindow, IHasCustomMenu
{
    private List<Object> selectionHistory = new List<Object>();
    private int currentIndex = -1;
    private string[] historyNames = new string[0];

    public void ClearSelectionHistory()
    {
        selectionHistory.Clear();
        currentIndex = -1;
        UpdateHistoryNames();
        isNavigating = false;
    }

    [MenuItem("Window/Selection History")]
    public static void ShowWindow()
    {
        var window = GetWindow<SelectionHistoryWindow>("Selection History");
        window.ShowTab();
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (!isNavigating)
        {
            if (Selection.activeObject != null && (selectionHistory.Count == 0 || selectionHistory[selectionHistory.Count - 1] != Selection.activeObject))
            {
                // Quando um novo objeto é selecionado, removemos tudo o que vem depois do currentIndex
                if (currentIndex < selectionHistory.Count - 1)
                {
                    selectionHistory.RemoveRange(currentIndex + 1, selectionHistory.Count - (currentIndex + 1));
                }

                selectionHistory.Add(Selection.activeObject);
                currentIndex = selectionHistory.Count - 1;
                UpdateHistoryNames();
            }
        }
    }


    bool isNavigating = false;

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        // Botão de "voltar" (←)
        GUI.enabled = currentIndex > 0;
        if (GUILayout.Button("←", GUILayout.Width(32), GUILayout.Height(24)))
        {
            isNavigating = true;
            currentIndex = Mathf.Clamp(currentIndex - 1, 0, selectionHistory.Count - 1);
            SelectCurrent();
        }

        // Botão de "avançar" (→)
        GUI.enabled = currentIndex < selectionHistory.Count - 1;
        if (GUILayout.Button("→", GUILayout.Width(32), GUILayout.Height(24)))
        {
            isNavigating = true;
            currentIndex = Mathf.Clamp(currentIndex + 1, 0, selectionHistory.Count - 1);
            SelectCurrent();
        }

        GUI.enabled = true;

        // Caixa de seleção para escolher diretamente o item do histórico
        int newIndex = EditorGUILayout.Popup(currentIndex, historyNames, GUILayout.Width(150));
        if (newIndex != currentIndex)
        {
            currentIndex = newIndex;
            SelectCurrent();
        }

        GUILayout.EndHorizontal();
    }

    private void OnLostFocus()
    {
        isNavigating = false;
    }


    private void SelectCurrent()
    {
        if (currentIndex >= 0 && currentIndex < selectionHistory.Count)
        {
            Selection.activeObject = selectionHistory[currentIndex];
        }
    }

    private void UpdateHistoryNames()
    {
        historyNames = selectionHistory.ConvertAll(obj => obj ? obj.name : "[Objeto Removido]").ToArray();
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Clear Selection History"), false, ClearSelectionHistory);
    }
}
