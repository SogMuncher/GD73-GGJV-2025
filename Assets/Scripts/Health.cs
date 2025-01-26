using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float _maxHealth;

    [SerializeField]
    private float _startingHealth;

    [SerializeField, ReadOnly]
    private float _currentHealth;

    [HideInInspector]
    public UnityEvent OnDeathEvent;

    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
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
        if (_currentHealth <= 0)
        {
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
