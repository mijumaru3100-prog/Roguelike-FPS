using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Gun/Sound")]
public class SoundsPlayer : ScriptableObject
{
    public List<SoundLayer> Sounds = new List<SoundLayer>();

    private float pitchRandomness = 0.1f;

    public void PlaySound(GunBase gun)
    {
        float pitch =
            1.0f + Random.Range(-pitchRandomness, pitchRandomness);

        gun.shotAudioSources[0].pitch = pitch;

        foreach (var sound in Sounds)
        {
            gun.StartCoroutine(PlayLayer(sound, gun));
        }
    }

    private IEnumerator PlayLayer(SoundLayer sound, GunBase gun)
    {
        if (sound.delay > 0f)
            yield return new WaitForSeconds(sound.delay);

        if (sound.clips == null || sound.clips.Length == 0)
            yield break;

        AudioClip clip =
            sound.clips[Random.Range(0, sound.clips.Length)];

        gun.shotAudioSources[0].PlayOneShot(
            clip,
            sound.volume
        );
    }
}