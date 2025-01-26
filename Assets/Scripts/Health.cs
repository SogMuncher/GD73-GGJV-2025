using FMODUnity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using FMOD.Studio;
using FMODUnity;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float _maxHealth = 5f;

    [SerializeField]
    private float _startingHealth = 3f;

    [SerializeField, ReadOnly]
    private float _currentHealth = 3f;

    [HideInInspector]
    public UnityEvent OnDeathEvent;
    
    [SerializeField]
    EventReference _takeDamageSFX;
    EventInstance _takeDamageInstance;

    [SerializeField] 
    EventReference _popSFX;

    [HideInInspector]
    public UnityEvent OnDamagedEvent;

    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        _currentHealth = _startingHealth;
    }

    public void ResetHealth()
    {
        _currentHealth = _startingHealth;
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    public void AddHealth(float addAmount)
    {
        _currentHealth += addAmount;

        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        OnDamagedEvent.Invoke();

        if (_currentHealth <= 0)
        {
            RuntimeManager.PlayOneShot("_popSFX", transform.position); // Play pop sound when health reaches 0
            Die();
        }
    }

    protected virtual void Die()
    {
        //death logic here
        OnDeathEvent.Invoke();
        int playerIndex = _playerInput.playerIndex;

        if (GameManager.instance != null && playerIndex >= 0)
        {
            GameManager.instance.IncrementPlayerScore(playerIndex);
        }
        
    }
}
