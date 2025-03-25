using UnityEditor;
using UnityEngine;

public class GroupObjects : Editor
{
    // Guardar o último grupo criado para não criar grupos dentro de grupos
    static GameObject lastObjectOperation;

    [MenuItem("GameObject/Group Objects", false, 0)]
    public static void GroupObjectsSelected()
    {
        if (Selection.activeGameObject != lastObjectOperation)
        {
            lastObjectOperation = null;
        }

        if (lastObjectOperation != null)
            return;
        // Criar um novo objeto para o grupo
        GameObject go = new GameObject($"[Group] ({Selection.transforms.Length}) {Random.Range(0, 10000)}");

        // Calcular o centro do grupo
        Vector3 goCenter = Vector3.zero;
        foreach (Transform t in Selection.transforms)
        {
            goCenter += t.position;
        }
        go.transform.position = goCenter / Selection.transforms.Length;

        // Definir o grupo como filho do mesmo objeto pai que o primeiro selecionado
        go.transform.parent = Selection.transforms[0].parent;
        go.transform.SetSiblingIndex(Selection.transforms[0].GetSiblingIndex());

        // Agrupar os objetos selecionados
        foreach (Transform t in Selection.transforms)
        {
            t.parent = go.transform;
        }

        Selection.activeGameObject = go;
        lastObjectOperation = go;
    }
}
