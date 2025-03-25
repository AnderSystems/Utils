using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [System.Serializable]
    public enum playType
    {
        sequential, random
    }
    [SerializeField]
    public playType type = playType.sequential;
    public AudioSource source;
    public List<AudioClip> clips = new List<AudioClip>();
    public int currentClip { get; set; }

    public void Play(int current)
    {
        currentClip = current;
        source.clip = clips[currentClip];
        source.Play();
        Invoke(nameof(Next), source.clip.length);
    }

    public void Next()
    {
        switch (type)
        {
            case playType.sequential:
                if (currentClip < clips.Count - 1)
                    Play(currentClip + 1);
                else
                    Play(0);
                break;
            case playType.random:
                Play(Random.Range(0, clips.Count));
                break;
        }
    }

    private void OnEnable()
    {
        CancelInvoke(nameof(Next));
        switch (type)
        {
            case playType.sequential:
                source.clip = clips[0];
                Play(0);
                break;
            case playType.random:
                Play(Random.Range(0, clips.Count));
                break;
        }
    }
}
