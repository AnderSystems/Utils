using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ComponentReplacer
{
    [MenuItem("CONTEXT/Component/Replace")]
    private static void ReplaceComponent(MenuCommand command)
    {
        Component originalComponent = (Component)command.context;

        // Abrir uma janela para selecionar o novo tipo de componente
        var window = EditorWindow.GetWindow<ReplaceComponentWindow>("Replace Component");
        window.Initialize(new List<Component> { originalComponent });
    }

    [MenuItem("GameObject/Replace Component", false, 20)]
    private static void ReplaceMultipleComponents(MenuCommand command)
    {
        List<Component> selectedComponents = new List<Component>();

        foreach (GameObject obj in Selection.gameObjects)
        {
            Component comp = obj.GetComponent<Component>();
            if (comp != null)
                selectedComponents.Add(comp);
        }

        if (selectedComponents.Count == 0)
        {
            Debug.LogWarning("No valid components selected for replacement.");
            return;
        }

        var window = EditorWindow.GetWindow<ReplaceComponentWindow>("Replace Component");
        window.Initialize(selectedComponents);
    }
}

public class ReplaceComponentWindow : EditorWindow
{
    private List<Component> originalComponents;
    private Vector2 scrollPosition;
    private List<Type> componentTypes;
    private List<Type> filteredComponentTypes;
    private string searchQuery = "";

    public void Initialize(List<Component> originals)
    {
        originalComponents = originals;
        LoadComponentTypes();
        FilterComponents();
    }

    private void LoadComponentTypes()
    {
        componentTypes = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Component)) && !type.IsAbstract)
                {
                    componentTypes.Add(type);
                }
            }
        }
        componentTypes.Sort((a, b) => a.Name.CompareTo(b.Name));
    }

    private void FilterComponents()
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            filteredComponentTypes = new List<Type>(componentTypes);
        }
        else
        {
            filteredComponentTypes = componentTypes.FindAll(type =>
                type.Name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    private void OnGUI()
    {
        if (originalComponents == null || originalComponents.Count == 0)
        {
            EditorGUILayout.HelpBox("No components selected for replacement.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Replacing Component(s):", EditorStyles.boldLabel);

        foreach (var comp in originalComponents)
        {
            EditorGUILayout.LabelField($"• {comp.GetType().Name} on {comp.gameObject.name}");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Search New Component", EditorStyles.boldLabel);
        string newSearchQuery = EditorGUILayout.TextField(searchQuery);

        if (newSearchQuery != searchQuery)
        {
            searchQuery = newSearchQuery;
            FilterComponents();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var type in filteredComponentTypes)
        {
            if (GUILayout.Button(type.Name))
            {
                Replace(type);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Replace(Type newComponentType)
    {
        Undo.RegisterCompleteObjectUndo(originalComponents[0].gameObject, "Replace Components");

        foreach (var originalComponent in originalComponents)
        {
            GameObject gameObject = originalComponent.gameObject;

            // Criar o novo componente
            Component newComponent = gameObject.AddComponent(newComponentType);

            // Copiar propriedades
            EditorUtility.CopySerialized(originalComponent, newComponent);

            // Substituir referências
            ReplaceReferences(originalComponent, newComponent);

            // Remover o componente original
            Undo.DestroyObjectImmediate(originalComponent);

            Debug.Log($"Replaced {originalComponent.GetType().Name} with {newComponentType.Name} on {gameObject.name}");
        }

        Close();
    }

    private void ReplaceReferences(Component oldComponent, Component newComponent)
    {
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            foreach (Component component in obj.GetComponents<Component>())
            {
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == oldComponent)
                    {
                        property.objectReferenceValue = newComponent;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
}
