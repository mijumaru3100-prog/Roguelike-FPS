using UnityEngine;

[System.Serializable]
public class SoundLayer
{
    public AudioClip[] clips;

    public float delay;

    [Range(0f, 1f)]
    public float volume = 1f;
}