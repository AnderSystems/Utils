using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class FastObjectPlace : Editor
{
    static FastObjectPlace()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    private static void OnSceneGUI(SceneView sv)
    {
        if (EditorWindow.focusedWindow != SceneView.currentDrawingSceneView ||
            !UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            return;
        Event e = Event.current;
        if (Application.isPlaying && Application.isFocused)
            return;
        if (e.type == EventType.Used || Event.current.type == EventType.Used)
            return;
        if (Selection.gameObjects.Length > 0)
        {
            //Rotate object with ctrl
            if (e.isScrollWheel && e.control)
            {
                objectRotation += ((int)(e.delta.y / 2)) * 15;
                foreach (GameObject obj in Selection.gameObjects)
                {
                    obj.transform.rotation = Quaternion.Euler(obj.transform.eulerAngles.x, objectRotation, obj.transform.eulerAngles.z);
                }
                e.Use();
            }

            //Scale object with s
            if (e.isKey && e.keyCode == KeyCode.U && e.type == EventType.KeyDown && e.button != 1)
            {
                if (!ScalingObject)
                {
                    if (LastObjectScale == null)
                    {
                        LastObjectScale = new List<Vector3>();
                    }

                    //Undo.RecordObjects(Selection.transforms, "Scale Objects");
                    foreach (var obj in Selection.transforms)
                    {
                        LastObjectScale.Add(obj.localScale);
                    }
                    ScalingObject = true;
                }
                else
                {
                    for (int i = 0; i < Selection.transforms.Length; i++)
                    {
                        Selection.transforms[i].localScale = LastObjectScale[i];
                    }

                    LastObjectScale?.Clear();
                    ScalingObject = false;
                }
            }

            if (e.alt && e.isScrollWheel)
            {
                e.Use();
                float scaleFactor = e.delta.y * 0.1f; // Ajuste a sensibilidade do scroll aqui
                for (int i = 0; i < Selection.transforms.Length; i++)
                {
                    Vector3 newScale = Selection.transforms[i].localScale + Vector3.one * scaleFactor;
                    Selection.transforms[i].localScale = newScale; // Evita escala negativa
                }
            }

            if (ScalingObject)
            {
                e.Use();
                float scaleFactor = e.delta.y * 0.1f; // Ajuste a sensibilidade do scroll aqui
                for (int i = 0; i < Selection.transforms.Length; i++)
                {
                    Vector3 newScale = Selection.transforms[i].localScale + Vector3.one * scaleFactor;
                    Selection.transforms[i].localScale = newScale; // Evita escala negativa
                }

                if (e.button == 1 || (e.type == EventType.KeyUp && e.keyCode == KeyCode.Return))
                {
                    LastObjectScale?.Clear();
                    ScalingObject = false;
                }

                if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Escape)
                {
                    for (int i = 0; i < Selection.transforms.Length; i++)
                    {
                        Selection.transforms[i].localScale = LastObjectScale[i];
                    }
                    LastObjectScale?.Clear();
                    ScalingObject = false;
                }

                //if (e.isScrollWheel && e.type == EventType.ScrollWheel)
                //{
                //    e.Use();
                //    float scaleFactor = e.delta.y * 0.1f; // Ajuste a sensibilidade do scroll aqui
                //    for (int i = 0; i < Selection.transforms.Length; i++)
                //    {
                //        Vector3 newScale = Selection.transforms[i].localScale + Vector3.one * scaleFactor;
                //        Selection.transforms[i].localScale = Vector3.Max(newScale, Vector3.one * 0.1f); // Evita escala negativa
                //    }
                //}
                //else
                //{
                //    if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Escape)
                //    {
                //        for (int i = 0; i < Selection.transforms.Length; i++)
                //        {
                //            Selection.transforms[i].localScale = LastObjectScale[i];
                //        }
                //        LastObjectScale?.Clear();
                //        ScalingObject = false;
                //    }

                //    //if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Return || e.button == 0)
                //    //{
                //    //    LastObjectScale?.Clear();
                //    //    ScalingObject = false;
                //    //}
                //}
            }


            /*
            if (e.isKey && e.keyCode == KeyCode.S && e.type == EventType.KeyDown)
            {
                if (!ScalingObject)
                {
                    OnCancelMovingObject();
                    OnCancelRotatingObject();
                    OnStartScalingObject();
                    MovingObject = 0;
                    ScalingObject = true;
                }
                else
                {
                    OnCancelScalingObject();
                    ScalingObject = false;
                }
            }*/


            //Position object with G
            if (e.isKey && e.keyCode == KeyCode.G)
            {
                if (e.type == EventType.KeyDown)
                {
                    Undo.RecordObjects(Selection.transforms, "Position Object");
                }

                if (e.type == EventType.KeyUp)
                {
                    if (MovingObject == 0)
                    {
                        OnCancelRotatingObject();
                        //OnCancelScalingObject();
                        OnStartMovingObject();
                        RotatingObject = false;
                        MovingObject = 1;
                    }
                    else
                    {
                        if (MovingObject == 1)
                        {
                            MovingObject = 2;
                        }
                        else
                        {
                            OnCancelMovingObject();
                            MovingObject = 0;
                        }

                    }
                }

                e.Use();
            }

            //Perform position
            if ((MovingObject == 2 || (e.control && e.shift)) &&
                e.isKey && e.keyCode == KeyCode.F)
            {
                if (e.type == EventType.KeyDown)
                {
                    Undo.RecordObjects(Selection.transforms, "Position Object");
                }

                foreach (GameObject obj in Selection.gameObjects)
                {
                    PutObjectOnGround(obj.transform, e);
                    if (e.isKey && e.keyCode == KeyCode.Escape)
                    {
                        MovingObject = 0;
                        e.Use();
                    }
                }

                if ((e.button == 0 && e.type == EventType.MouseDown) || (e.isKey && e.keyCode == KeyCode.Return))
                {
                    MovingObject = 0;
                    LastObjectEulers = new List<Vector3>();
                    LastObjectPositions = new List<Vector3>();
                    e.Use();
                }


                if (e.isKey && e.keyCode == KeyCode.Escape)
                {
                    OnCancelMovingObject();
                    MovingObject = 0;
                }
            }
            if (MovingObject == 1)
            {
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    PutObjectOnGroundRelative(Selection.gameObjects[i].transform, e, LastObjectPositions[i]);
                }

                if (e.isKey && e.keyCode == KeyCode.Escape)
                {
                    MovingObject = 0;
                    e.Use();
                }

                if ((e.button == 0 && e.type == EventType.MouseDown) || (e.isKey && e.keyCode == KeyCode.Return))
                {
                    MovingObject = 0;
                    LastObjectEulers = new List<Vector3>();
                    LastObjectPositions = new List<Vector3>();
                    e.Use();
                }


                if (e.isKey && e.keyCode == KeyCode.Escape)
                {
                    OnCancelMovingObject();
                    MovingObject = 0;
                }
            }

            //Perform rotation
            if (RotatingObject)
            {
                if (e.isKey && e.keyCode == KeyCode.X && e.type == EventType.KeyDown)
                {
                    RotationAxis = rotationAxis.X;
                }
                if (e.isKey && e.keyCode == KeyCode.Y && e.type == EventType.KeyDown)
                {
                    RotationAxis = rotationAxis.Y;
                }
                if (e.isKey && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown)
                {
                    RotationAxis = rotationAxis.Y;
                }

                if (e.type == EventType.MouseMove)
                {
                    objectRotation = e.delta.x + e.delta.y;
                    foreach (var o in Selection.gameObjects)
                    {
                        RotateObject(o, RotationAxis);
                    }
                }

                if (e.isKey && e.keyCode == KeyCode.Escape)
                {
                    OnCancelRotatingObject();
                    RotatingObject = false;
                }

                if ((e.button == 0 && e.type == EventType.MouseDown) || (e.isKey && e.keyCode == KeyCode.Return))
                {
                    RotatingObject = false;
                    LastObjectEulers = new List<Vector3>();
                    LastObjectPositions = new List<Vector3>();
                    e.Use();
                }
            }
            else
            {
                RotationAxis = rotationAxis.Y;
            }
        }
        else
        {
            RotationAxis = rotationAxis.Y;
        }

        if (e.isKey && e.keyCode == KeyCode.Escape)
        {
            OnCancelMovingObject();
            OnCancelRotatingObject();
            //OnCancelScalingObject();
            MovingObject = 0;
            RotatingObject = false;
            ScalingObject = false;
        }
    }

    static float objectRotation;
    static int MovingObject;
    static bool RotatingObject;
    static bool ScalingObject;
    Vector2 mousePosition;
    Vector2 boundsCenter;

    static List<Vector3> LastObjectPositions = new List<Vector3>();
    static List<Vector3> LastObjectEulers = new List<Vector3>();
    static List<Vector3> LastObjectScale = new List<Vector3>();
    public enum rotationAxis
    {
        X,
        Y,
        Z
    }
    public static rotationAxis RotationAxis;
    public enum alingAxis
    {
        None,
        X,
        Y,
        Z
    }

    public static Bounds CalculateObjectOffset(Transform obj)
    {
        Bounds b = new Bounds(obj.position, Vector3.zero);

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
            b.Encapsulate(r.bounds);
        foreach (var c in colliders)
            b.Encapsulate(c.bounds);



        //return offset - (offset / 2);
        return b;
    }

    public static Vector3 PlaceObject(Transform obj, Vector3 normal)
    {
        Bounds b = CalculateObjectOffset(obj);
        b.center -= obj.position;  // Ajusta o centro para que a posição do objeto seja relativa ao seu próprio centro.
        Vector3 offset = Vector3.zero;

        Handles.color = Color.white;
        Handles.DrawWireCube(b.center + obj.position, b.size);
        Handles.DrawWireCube(b.center, b.size);

        // Normaliza a normal para garantir que ela é unitária
        normal.Normalize();

        // Calcula a projeção do centro do objeto em relação à normal.
        // Isso garante que vamos empurrar o objeto para a direção correta
        // sem que ele ultrapasse a superfície.
        float dotX = Vector3.Dot(normal, Vector3.right);
        float dotY = Vector3.Dot(normal, Vector3.up);
        float dotZ = Vector3.Dot(normal, Vector3.forward);

        // Calcula os offsets baseados na projeção dos limites
        offset.x = Mathf.Lerp(-b.max.x, -b.min.x, Mathf.Clamp01((1 + dotX) / 2));
        offset.y = Mathf.Lerp(-b.max.y, -b.min.y, Mathf.Clamp01((1 + dotY) / 2));
        offset.z = Mathf.Lerp(-b.max.z, -b.min.z, Mathf.Clamp01((1 + dotZ) / 2));

        return offset;
    }




    public static Vector3 VectorMultipiler(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static Vector3 CoordsToGrid(Vector3 original, Vector3 gridSize)
    {
        return new Vector3(
            Mathf.Round(original.x / gridSize.x) * gridSize.x,
            Mathf.Round(original.y / gridSize.y) * gridSize.y,
            Mathf.Round(original.z / gridSize.z) * gridSize.z
        );
    }

    public static void PutObjectOnGround(Transform obj, Event e)
    {
        obj.gameObject.SetActive(false);

        RaycastHit hit;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out hit, 9999f))
        {
            if (hit.collider)
            {
                if (e.control)
                {
                    obj.transform.position = CoordsToGrid(hit.point + PlaceObject(obj, hit.normal), EditorSnapSettings.gridSize);
                }
                else
                {
                    obj.transform.position = hit.point + PlaceObject(obj, hit.normal);
                }
            }
        }

        obj.gameObject.SetActive(true);
    }
    public static void PutObjectOnGroundRelative(Transform obj, Event e, Vector3 LastPosition)
    {
        obj.gameObject.SetActive(false);

        RaycastHit hit;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out hit, 9999f))
        {
            if (hit.collider)
            {
                //Vector3 relativeHit = hit.point - LastPosition;

                if (e.control)
                {
                    obj.transform.position = CoordsToGrid(hit.point + PlaceObject(obj, hit.normal), EditorSnapSettings.gridSize);
                }
                else
                {
                    obj.transform.position = hit.point + PlaceObject(obj, hit.normal);
                }
            }
        }

        obj.gameObject.SetActive(true);
    }

    static void OnStartMovingObject()
    {
        LastObjectPositions = new List<Vector3>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            LastObjectPositions.Add(obj.transform.position);
        }
    }
    static void OnCancelMovingObject()
    {
        for (int i = 0; i < LastObjectPositions.Count; i++)
        {
            Selection.gameObjects[i].transform.position = LastObjectPositions[i];
        }
    }

    static void OnStartRotatingObject()
    {
        LastObjectEulers = new List<Vector3>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            LastObjectEulers.Add(obj.transform.eulerAngles);
        }
    }
    static void OnCancelRotatingObject()
    {
        for (int i = 0; i < LastObjectEulers.Count; i++)
        {
            Selection.gameObjects[i].transform.eulerAngles = LastObjectEulers[i];
        }
    }

    static void RotateObject(GameObject obj, rotationAxis axis)
    {
        /*Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 99999);
        Quaternion q = Quaternion.LookRotation(hit.point);*/
        switch (axis)
        {
            case rotationAxis.X:
                obj.transform.eulerAngles -= new Vector3(objectRotation, 0, 0);
                break;
            case rotationAxis.Y:
                //obj.transform.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
                obj.transform.eulerAngles -= new Vector3(0, objectRotation, 0);
                break;
            case rotationAxis.Z:
                obj.transform.eulerAngles -= new Vector3(0, 0, objectRotation);
                break;
            default:
                break;
        }
    }
}