using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class DartParticleShooter : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private Particle[] _particles;
    private EventReference _takeDamageSFX;
    private EventReference _impactSFX;
    private EventReference _spikeToSpikeSFX;

    private DartShooterSwitch _dartShooterSwitch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
        _dartShooterSwitch = FindFirstObjectByType<DartShooterSwitch>();
    }

    private void Start()
    {
        if (_dartShooterSwitch != null)
        {
            _dartShooterSwitch.OnSwitchOn.AddListener(StartShooting);
            _dartShooterSwitch.OnSwitchOff.AddListener(StopShooting);
        }
    }

    private void StartShooting()
    {
        _particleSystem?.Play();
    }

    private void StopShooting()
    {
        _particleSystem?.Stop();
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("Dart Particle Collided!");
        if (other.TryGetComponent(out Health health))
        {
            int numParticlesAlive = _particleSystem.GetParticles(_particles);
            for (int i = 0; i < numParticlesAlive; i++)
            {
                Vector3 particlePosition = _particles[i].position;
                Vector3 collisionPoint = other.transform.position;
                float distance = Vector3.Distance(particlePosition, collisionPoint);

                if (distance <= 1.0f)
                {
                    _particles[i].remainingLifetime = 0;
                    _particleSystem.SetParticles(_particles, numParticlesAlive);
                    RuntimeManager.PlayOneShot(_takeDamageSFX, transform.position);
                    health.TakeDamage(1, transform.position);
                }

                RuntimeManager.PlayOneShot(_impactSFX, transform.position);

            }

        }

        if (other.TryGetComponent(out ThrownWeapon otherWeapon))
        {
            int numParticlesAlive = _particleSystem.GetParticles(_particles);
            for (int i = 0; i < numParticlesAlive; i++)
            {
                Vector3 particlePosition = _particles[i].position;
                Vector3 collisionPoint = other.transform.position;
                float distance = Vector3.Distance(particlePosition, collisionPoint);

                if (distance <= 1.5f)
                {
                    _particles[i].remainingLifetime = 0;
                    _particleSystem.SetParticles(_particles, numParticlesAlive);
                    RuntimeManager.PlayOneShot(_spikeToSpikeSFX, transform.position); // Play spike to spike SFX when two thrown weapons collide
                    otherWeapon.DestroyObject();
                }

                RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision
            }

        }

    }
}
