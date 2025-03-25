using UnityEditor;
using UnityEngine;

public class RandomizeTransformWindow : EditorWindow
{
    private Vector3 positionRange = Vector3.zero;
    private Vector3 rotationRange = Vector3.zero;
    private Vector3 scaleRange = Vector3.one;

    private Transform[] selectedTransforms;
    private Vector3[] originalPositions;
    private Vector3[] originalRotations;
    private Vector3[] originalScales;

    [MenuItem("CONTEXT/Transform/Randomize Transform")]
    private static void OpenWindow(MenuCommand command)
    {
        var window = GetWindow<RandomizeTransformWindow>("Randomize Transform");
        window.selectedTransforms = Selection.transforms;

        // Store the original transforms
        int count = window.selectedTransforms.Length;
        window.originalPositions = new Vector3[count];
        window.originalRotations = new Vector3[count];
        window.originalScales = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            var t = window.selectedTransforms[i];
            window.originalPositions[i] = t.position;
            window.originalRotations[i] = t.eulerAngles;
            window.originalScales[i] = t.localScale;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Randomization Ranges", EditorStyles.boldLabel);
        positionRange = EditorGUILayout.Vector3Field("Position Range", positionRange);
        rotationRange = EditorGUILayout.Vector3Field("Rotation Range", rotationRange);
        scaleRange = EditorGUILayout.Vector3Field("Scale Range", scaleRange);

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply Randomization"))
        {
            ApplyRandomization();
        }

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {
            SaveChanges();
            Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            RevertChanges();
            Close();
        }
        GUILayout.EndHorizontal();
    }

    private void ApplyRandomization()
    {
        for (int i = 0; i < selectedTransforms.Length; i++)
        {
            var t = selectedTransforms[i];
            t.position = originalPositions[i] + new Vector3(
                Random.Range(-positionRange.x, positionRange.x),
                Random.Range(-positionRange.y, positionRange.y),
                Random.Range(-positionRange.z, positionRange.z)
            );

            t.eulerAngles = originalRotations[i] + new Vector3(
                Random.Range(-rotationRange.x, rotationRange.x),
                Random.Range(-rotationRange.y, rotationRange.y),
                Random.Range(-rotationRange.z, rotationRange.z)
            );

            t.localScale = originalScales[i] + new Vector3(
                Random.Range(-scaleRange.x, scaleRange.x),
                Random.Range(-scaleRange.y, scaleRange.y),
                Random.Range(-scaleRange.z, scaleRange.z)
            );
        }
    }

    private void SaveChanges()
    {
        Undo.RecordObjects(selectedTransforms, "Randomize Transform");
    }

    private void RevertChanges()
    {
        for (int i = 0; i < selectedTransforms.Length; i++)
        {
            var t = selectedTransforms[i];
            t.position = originalPositions[i];
            t.eulerAngles = originalRotations[i];
            t.localScale = originalScales[i];
        }
    }
}
