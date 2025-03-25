using UnityEngine;

public class DisableObjectBeforeStart : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        gameObject.SetActive(false);
    }
}
