using UnityEngine;

public class NoClip : MonoBehaviour
{
    public static bool NoClipEnabled;
    public Entity target { get; set; }
    public float defaultSpeed = 1f;
    float currentSpeed = 1f;
    Vector3 inputs
    {
        get
        {
            Vector3 result = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                result += Vector3.forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                result += Vector3.back;
            }

            if (Input.GetKey(KeyCode.A))
            {
                result += Vector3.left;
            }

            if (Input.GetKey(KeyCode.D))
            {
                result += Vector3.right;
            }

            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.E))
            {
                result += Vector3.up;
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Q))
            {
                result += Vector3.down;
            }

            return result;
        }
    }

    public void EnableNoClip(Entity entity)
    {
        NoClipEnabled = true;
        target = entity;
    }
    public void DisableNoClip()
    {
        NoClipEnabled = false;
        target = null;
    }

    //Mono
    private void Update()
    {
        if (NoClipEnabled && Input.GetKeyDown(KeyCode.Return))
        {
            DisableNoClip();
        }
    }
    private void FixedUpdate()
    {
        if (target && NoClipEnabled)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed += 0.1f;
            }
            else
            {
                currentSpeed = defaultSpeed;
            }

            if (Input.mouseScrollDelta.y > 0)
            {
                defaultSpeed += 0.1f;
            }

            if (defaultSpeed > 0.01f)
            {
                if (Input.mouseScrollDelta.y < 0)
                {
                    defaultSpeed -= 0.1f;
                }
            }

            target.transform.forward = Camera.main.transform.forward;
            target.transform.eulerAngles = new Vector3(0, target.transform.eulerAngles.y, 0);



            Debug.Log("[oClip]" + "NoClip Enabled!", this);
            target.Freeze();
            target.transform.position += Camera.main.transform.TransformVector(inputs) * currentSpeed;
        }
    }
}
