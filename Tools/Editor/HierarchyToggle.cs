using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyToggle
{
    static HierarchyToggle()
    {
        // Adiciona a fun��o ao evento de desenho da Hierarquia
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        // Obt�m o objeto pelo ID da inst�ncia
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (obj != null)
        {
            // Define a posi��o do toggle antes do nome
            Rect toggleRect = new Rect(selectionRect.x, selectionRect.y, 18, selectionRect.height);

            // Desenha o toggle e altera o estado ativo do objeto
            bool isActive = GUI.Toggle(toggleRect, obj.activeSelf, GUIContent.none);

            if (isActive != obj.activeSelf)
            {
                Undo.RecordObject(obj, "Toggle Active State");
                obj.SetActive(isActive);
            }
        }
    }
}
