using UnityEngine;

public class LerpUtils : MonoBehaviour
{
    public static float LinearLerp(float currentValue, float targetValue, float speed, float deltaTime)
    {
        float step = speed * deltaTime;

        if (Mathf.Abs(targetValue - currentValue) <= step)
            return targetValue;

        return currentValue + Mathf.Sign(targetValue - currentValue) * step;
    }

    public static float SmoothInOutLerp(float currentValue, float targetValue, float speed, float deltaTime)
    {
        float distance = Mathf.Abs(targetValue - currentValue);
        float step = speed * deltaTime / distance;
        step = Mathf.Clamp01(step);

        float smoothStep = Mathf.SmoothStep(0, 1, step);

        return Mathf.Lerp(currentValue, targetValue, smoothStep);
    }

    public static Vector3 Vector3LinearLerp(Vector3 currentValue, Vector3 targetValue, float speed, float deltaTime)
    {
        return new Vector3(
            LinearLerp(currentValue.x, targetValue.x, speed, deltaTime),
            LinearLerp(currentValue.y, targetValue.y, speed, deltaTime),
            LinearLerp(currentValue.z, targetValue.z, speed, deltaTime)
        );
    }

    public static Vector3 Vector3SmoothLerp(Vector3 currentValue, Vector3 targetValue, float speed, float deltaTime)
    {
        return new Vector3(
            SmoothInOutLerp(currentValue.x, targetValue.x, speed, deltaTime),
            SmoothInOutLerp(currentValue.y, targetValue.y, speed, deltaTime),
            SmoothInOutLerp(currentValue.z, targetValue.z, speed, deltaTime)
        );
    }

    public static Vector3 Vector3LinearAngleLerp(Vector3 currentValue, Vector3 targetValue, float speed, float deltaTime)
    {
        return new Vector3(
            Mathf.LerpAngle(currentValue.x, targetValue.x, speed * deltaTime),
            Mathf.LerpAngle(currentValue.y, targetValue.y, speed * deltaTime),
            Mathf.LerpAngle(currentValue.z, targetValue.z, speed * deltaTime)
        );
    }

    public static Vector3 Vector3SmoothAngleLerp(Vector3 currentValue, Vector3 targetValue, float speed, float deltaTime)
    {
        return new Vector3(
            Mathf.LerpAngle(currentValue.x, targetValue.x, Mathf.SmoothStep(0, 1, speed * deltaTime)),
            Mathf.LerpAngle(currentValue.y, targetValue.y, Mathf.SmoothStep(0, 1, speed * deltaTime)),
            Mathf.LerpAngle(currentValue.z, targetValue.z, Mathf.SmoothStep(0, 1, speed * deltaTime))
        );
    }
}
