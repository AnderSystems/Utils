using UnityEngine;

public class MoveObjectLegacy : MonoBehaviour
{
    public string id = "";
    public bool SaveObject;

    public Transform target;
    public Transform posA;
    public Transform posB;
    public bool ApplyScale;
    [System.Serializable]
    public enum extrapolation
    {
        Default, PingPong
    }
    [SerializeField] public extrapolation Extrapolation;

    public float timeline { get; set; }
    public float defaultTransitionTime = 3;
    public float currentTransitionTime { get; set; }
    public bool playOnEnable = true;
    public bool Loop;
    public bool isPlaying { get; set; }
    int direction = 1;
    public void Play(float transitionTime)
    {
        currentTransitionTime = transitionTime;
        isPlaying = true;
    }
    public void Play()
    {
        Play(defaultTransitionTime);
    }
    public void PlayReverse()
    {
        direction = -1;
        Play(defaultTransitionTime);
    }
    public void Pause()
    {
        isPlaying = false;
    }
    public void Stop()
    {
        timeline = 0;
        isPlaying = false;
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Stop();
            Play();
        }
    }
    private void LateUpdate()
    {
        if (isPlaying)
        {
            timeline += (currentTransitionTime * Time.deltaTime) * direction;


            target.transform.position = Vector3.Lerp(posA.position, posB.position, timeline);
            target.transform.rotation = Quaternion.Lerp(posA.rotation, posB.rotation, timeline);
            if (ApplyScale)
            {
                target.transform.localScale = Vector3.Lerp(posA.localScale, posB.localScale, timeline); ;
            }


            if (timeline >= 1 || timeline <= 0)
            {
                if (Extrapolation == extrapolation.Default)
                {
                    if (Loop)
                    {
                        timeline = 0;
                    }
                }

                if (Extrapolation == extrapolation.PingPong)
                {
                    direction *= -1;
                }

                timeline = Mathf.Clamp(timeline, 0f, 1f);

                if (!Loop)
                {
                    isPlaying = false;
                }
            }
        }
    }

    bool editor_perform;
    public void OnDrawGizmos()
    {
        if (id == "" && gameObject.scene != null)
        {
            id = gameObject.GetInstanceID() + "_" + gameObject.name;
        }

#if UNITY_EDITOR

        if (UnityEditor.Selection.activeGameObject == posA.gameObject)
        {
            target.transform.position = posA.transform.position;
            target.transform.rotation = posA.transform.rotation;

            if (ApplyScale)
            {
                target.transform.localScale = posA.transform.localScale;
            }

            editor_perform = true;
        }
        else
        {
            if (UnityEditor.Selection.activeGameObject == posB.gameObject)
            {
                target.transform.position = posB.transform.position;
                target.transform.rotation = posB.transform.rotation;


                if (ApplyScale)
                {
                    target.transform.localScale = posB.transform.localScale;
                }

                editor_perform = true;
            }
            else
            {
                if (editor_perform)
                {
                    target.transform.position = posA.transform.position;
                    target.transform.rotation = posA.transform.rotation;

                    if (ApplyScale)
                    {
                        target.transform.localScale = posA.transform.localScale;
                    }

                    editor_perform = false;
                }
            }
        }

#endif
    }


    private void OnValidate()
    {

    }
}
