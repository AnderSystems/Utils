using UnityEngine;
using UnityEngine.Events;

public class CustomObjectActivation : MonoBehaviour
{
    public Vector3 startScale { get; set; }
    public bool isDisplayed { get { return transform.localScale.magnitude > 0; } }
    public float BlinkSpeed = .3f;

    [Header("Activation")]
    public bool BlinkOnActivate;
    public float ActivationDelay;
    public UnityEvent OnActivateEvent;

    [Header("Deactivation")]
    public bool BlinkOnDeactivate;
    public float DeactivationDelay;
    public UnityEvent OnDeactivateEvent;


    [Header("On Became Invisible")]
    public float OnBecameInvisibleDelay;
    public UnityEvent OnBecameInvisibleEvent;

    public void OnEnable()
    {
        Activate();
    }

    public void GetStartScale()
    {
        if (transform.localScale.magnitude > 0)
        {
            startScale = transform.localScale;
        }
    }

    public void OnBecameInvisible()
    {
        Invoke("CallOnBecameInvisibleEvent", OnBecameInvisibleDelay);
    }

    private void OnBecameVisible()
    {
        CancelInvoke("CallOnBecameInvisibleEvent");
    }
    public void CallOnBecameInvisibleEvent()
    {
        OnBecameInvisibleEvent.Invoke();
    }

    public void Activate()
    {
        this.gameObject.SetActive(true);
        GetStartScale();
        if (BlinkOnActivate)
        {
            StartBlinking();
        }
        else
        {
            //transform.localScale = Vector3.zero;
        }

        Invoke("StopBlinking", ActivationDelay);
        Invoke("ForceActvate", ActivationDelay);
    }
    public void Deactivate()
    {
        GetStartScale();
        if (BlinkOnDeactivate)
        {
            StartBlinking();
        }
        else
        {
            transform.localScale = Vector3.zero;
        }

        Invoke("StopBlinking", DeactivationDelay);
        Invoke("ForceDeactivate", DeactivationDelay);
    }

    public void ForceActvate()
    {
        this.gameObject.SetActive(true);
        GetStartScale();
        OnActivateEvent?.Invoke();
        Show();
    }

    public void ForceDeactivate()
    {
        GetStartScale();
        OnDeactivateEvent.Invoke();
        this.gameObject.SetActive(false);
    }

    public void Hide()
    {
        GetStartScale();
        transform.localScale = Vector3.zero;
    }

    public void Show()
    {
        transform.localScale = Vector3.one;
        GetStartScale();
    }

    public void DestroyObject(GameObject obj)
    {
        DestroyObject(obj.gameObject);
    }

    public void StartBlinking()
    {
        if (isDisplayed)
        {
            Hide();
        }
        else
        {
            Show();
        }
        Invoke("StartBlinking", BlinkSpeed);
    }

    public void StopBlinking()
    {
        CancelInvoke("StartBlinking");
        Show();
    }
}
