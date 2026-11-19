using UnityEngine;

public class HelicopterMissile : MonoBehaviour
{

    public int damage = 1;

    [Header("Effects")]
[SerializeField] private ParticleSystem ExplosionEffect;

    public float flightTime = 2f;

private Vector3 targetPosition;
private Vector3 startPosition;
private float timer;

public void SetTarget(Vector3 target)
{
    targetPosition = target;
    startPosition = transform.position;
    timer = 0f;
}

private void Update()
{
    timer += Time.deltaTime;

    float t = Mathf.Clamp01(timer / flightTime);

    transform.position = Vector3.Lerp(
        startPosition,
        targetPosition,
        t
    );

    if (t >= 1.0f)
    {
        Explode();
    }
}

    private void Explode()
    {
        ExplosionEffect.Play();

        Destroy(gameObject,0.5f);
    }
}