using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGizmos : MonoBehaviour
{
    public static void Draw2DArrow(Vector3 pos, float size, Vector3 euler)
    {
        //Basic coords
        Vector3 right = new Vector3(Mathf.Cos(-euler.y * Mathf.Deg2Rad), 0, Mathf.Sin(-euler.y * Mathf.Deg2Rad)) * size;
        Vector3 fwd = new Vector3(Mathf.Sin(euler.y * Mathf.Deg2Rad), 0, Mathf.Cos(euler.y * Mathf.Deg2Rad)) * size;

        //Body coords
        Vector3 fwdBody = pos + (fwd / 3);


        //Right
        Gizmos.DrawLine(fwdBody + right, pos + (fwd * 1.5f));
        //Left
        Gizmos.DrawLine(fwdBody - right, pos + (fwd * 1.5f));

    }
}
