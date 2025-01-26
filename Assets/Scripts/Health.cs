using FMODUnity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMOD.Studio;
using System.Collections;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float _maxHealth = 5f;
    public float MaxHealth => _maxHealth;

    [SerializeField]
    private float _startingHealth = 3f;

    [SerializeField, ReadOnly]
    private float _currentHealth = 3f;

    [SerializeField]
    protected float _invulnerabilityTime = 1f;
    protected bool _canBeHurt = true;

    public bool CanBeHurt => _canBeHurt;
    [SerializeField] private GameObject _damagedAnim;
    [SerializeField] private GameObject _angryAnim;


    [HideInInspector]
    public UnityEvent OnDeathEvent;
    
    [SerializeField]
    EventReference _takeDamageSFX;
    EventInstance _takeDamageInstance;

    [SerializeField] 
    EventReference _popSFX;

    [SerializeField]
    private int _playerIndex;

    [HideInInspector]
    public UnityEvent<Vector3> OnDamagedEvent;

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

    public void TakeDamage(float damageAmount, Vector3 damagerPosition)
    {
        if (_canBeHurt == false)
        {
            return;   
        }

        _currentHealth -= damageAmount;
        OnDamagedEvent.Invoke(damagerPosition);

        int playerIndex = _playerInput.playerIndex;

        if (GameManager.instance != null && playerIndex >= 0)
        {
            GameManager.instance.IncrementPlayerScore(_playerIndex);
        }

        if (_currentHealth <= 0)
        {
            RuntimeManager.PlayOneShot(_popSFX, transform.position);
            Die();
        }
        else
        {
            // after taking damage start invulnerability tinme ONLY IF NOT DEAD
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }


    protected IEnumerator InvulnerabilityCoroutine()
    {
        _canBeHurt = false;
        _damagedAnim.SetActive(!_canBeHurt);
        _angryAnim.SetActive(!_canBeHurt);
        yield return new WaitForSeconds(_invulnerabilityTime);

        _canBeHurt = true;
        _damagedAnim.SetActive(!_canBeHurt);
        _angryAnim.SetActive(!_canBeHurt);
        yield break;
    }

    protected virtual void Die()
    {
        //death logic here
        OnDeathEvent.Invoke();
        
    }
}
