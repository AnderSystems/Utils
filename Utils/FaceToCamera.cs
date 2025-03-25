using UnityEngine;

public class FaceToCamera : MonoBehaviour
{
    public float Scale = 1.0f;
    public Transform target { get; set; }

    public bool RotateX = true;
    public bool RotateY = true;
    public bool RotateZ = true;

    public void Perform(Transform customTarget)
    {
        Vector3 euler = transform.eulerAngles;

        if (RotateX)
            euler.x = customTarget.eulerAngles.x;

        if (RotateY)
            euler.y = customTarget.eulerAngles.y;

        if (RotateZ)
            euler.z = customTarget.eulerAngles.z;


        transform.eulerAngles = euler;
        if (Scale > 0f)
        {
            float scale = Vector3.Distance(customTarget.position, transform.position) * Scale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void Perform()
    {
        Perform(target);
    }

    private void OnEnable()
    {
        target = RenderCam.main.transform;
        Perform();
    }
    private void Awake()
    {
        target = RenderCam.main.transform;
        Perform();
    }
    public void Update()
    {
        Perform();
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
        Perform(Camera.current.transform);
    }
}
