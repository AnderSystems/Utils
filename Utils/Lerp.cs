using UnityEngine;

public class Lerp : MonoBehaviour
{
    /// <summary>
    /// Linear lerp between values
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="End"></param>
    /// <param name="Lerp"></param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public static float LinearFloat(float Value, float End, float Lerp, float deltaTime)
    {
        float lerpingDiference = Mathf.Abs(Value - End);

        return Mathf.Lerp(Value, End, (Lerp * deltaTime) * lerpingDiference);

        //Quanto mais longe de 1, mais lento, ou seja mais proximo de 0
        //Quanto mais perto de 1, mais rápido, ou seja mais proximo de 1
    }
    /// <summary>
    /// Performs a linear interpolation (lerp) while maintaining a constant speed 
    /// between values, regardless of the distance between them.
    /// </summary>
    /// <param name="Value">The starting value.</param>
    /// <param name="End">The target value.</param>
    /// <param name="Lerp">The interpolation speed.</param>
    /// <param name="deltaTime">The time step to ensure frame rate independence.</param>
    /// <returns>The interpolated value.</returns>
    public static float DistanceLerp(float Value, float End, float Lerp, float deltaTime)
    {
        float distance = Mathf.Abs(Value - End);
        if (distance < Mathf.Epsilon) return End; // Avoid division by zero
        return Mathf.Lerp(Value, End, ((Lerp * 2) / distance) * deltaTime);
    }

    /// <summary>
    /// Performs a linear interpolation (lerp) while maintaining a constant speed 
    /// between values, regardless of the distance between them.
    /// </summary>
    /// <param name="Value">The starting value.</param>
    /// <param name="End">The target value.</param>
    /// <param name="Lerp">The interpolation speed.</param>
    /// <param name="deltaTime">The time step to ensure frame rate independence.</param>
    /// <returns>The interpolated value.</returns>
    public static Vector3 DistanceLerp(Vector3 Value, Vector3 End, float Lerp, float deltaTime)
    {
        float distance = (Value - End).sqrMagnitude;
        if (distance < Mathf.Epsilon) return End; // Avoid division by zero
        return Vector3.Lerp(Value, End, ((Lerp * 2) / Mathf.Sqrt(distance)) * deltaTime);
    }

}
