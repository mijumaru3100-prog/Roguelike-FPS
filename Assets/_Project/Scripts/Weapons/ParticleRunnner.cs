using UnityEngine;
using System.Collections;

public class PerticleRunner : MonoBehaviour
{
    public static PerticleRunner Instance;

    void Awake()
    {
        Instance = this;
    }

    public static void Play(ParticleSystem ps, float duration)
    {
        Instance.StartCoroutine(Run(ps, duration));
    }

    static IEnumerator Run(ParticleSystem ps, float duration)
{
    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    ps.Play();

    yield return new WaitForSeconds(duration);

    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
}
}