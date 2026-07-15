using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SoulFlow : MonoBehaviour
{
    public Transform target;
    public float speed = 6f;

    ParticleSystem ps;
    ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        int count = ps.particleCount;

        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = (target.position - particles[i].position).normalized;
            particles[i].velocity = dir * speed;
        }

        ps.SetParticles(particles, count);
    }
}