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
    private int _maxHealth = 5;
    public int MaxHealth => _maxHealth;

    [SerializeField]
    private int _startingHealth = 3;

    [SerializeField, ReadOnly]
    private int _currentHealth = 3;

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
        _takeDamageInstance = RuntimeManager.CreateInstance(_takeDamageSFX); //create an instance of the take damage SFX
    }

    public void ResetHealth()
    {
        _currentHealth = _startingHealth;
        _takeDamageInstance.setParameterByName("Damage", _startingHealth);
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public void AddHealth(int addAmount)
    {
        _currentHealth += addAmount;

        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    public void TakeDamage(int damageAmount, Vector3 damagerPosition)
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
            //GameManager.instance.IncrementPlayerScore(_playerIndex);
            GameManager.instance.UpdatePlayerHealth(_playerIndex);
            
            _takeDamageInstance.setParameterByName("Damage", _currentHealth); // set the takedamage SFX parameter to the current health
            _takeDamageInstance.start(); //play takedamage SFX on hit
        }
      
        if (_currentHealth <= 0)
        {
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
        RuntimeManager.PlayOneShot(_popSFX);
        _takeDamageInstance.release(); //stop the take damage SFX - prevents from creating multiple instances of the same sound
        OnDeathEvent.Invoke();
        
    }
}
