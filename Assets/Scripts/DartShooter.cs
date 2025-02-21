using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DartShooter : MonoBehaviour
{
    private bool _isShooting;
    private bool _isTiltedUp;
    private bool _isTiltedDown;
    [SerializeField] private bool _canShoot = false;

    private GameObject _lastDartShot;
    [SerializeField] private GameObject _dartShot;
    [SerializeField] private Transform _dartSpawnPoint1;
    [SerializeField] private Transform _dartSpawnPoint2;
    [SerializeField] private Transform _dartSpawnPoint3;

    [SerializeField] private float _shootWaitTime = 0.5f;
    [SerializeField] private float _shootFrequency = 0.5f;
    [SerializeField] private float _shootStrenght;
    private Vector2 _aimInput;
    private Quaternion _shootAngle;
    private float _tiltUpAngle;
    private float _tiltDownAngle;

    private DartShooterSwitch _dartShooterSwitch;
    private GameManager _gameManager;

    private void Awake()
    {
        _dartShooterSwitch = FindFirstObjectByType<DartShooterSwitch>();
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _aimInput = gameObject.transform.right;
        _shootAngle = gameObject.transform.rotation;
        _dartShooterSwitch.OnSwitchOn.AddListener(CallShootDartsCoroutine);
        _gameManager.RoundStarting.AddListener(CallShootDartsCoroutine);
        _dartShooterSwitch.OnSwitchOff.AddListener(StopShooting);
    }

    
    public void CallShootDartsCoroutine()
    {
        _canShoot = !_canShoot;
        if (_isShooting == false)
        {
            _isShooting = true;
            StartCoroutine(ShootDartsCorroutine());                          
        }
    }

    public void StopShooting()
    {
        if (_isShooting == true)
        {
            StopAllCoroutines();
            _isShooting = false;
        }
        _canShoot = !_canShoot;
    }

    private IEnumerator ShootDartsCorroutine()
    {
        while (_canShoot)
        {
            ShootDarts(_dartSpawnPoint2.transform.position, _shootAngle);
            yield return new WaitForSeconds(_shootWaitTime);

            ShootDarts(_dartSpawnPoint3.transform.position, _shootAngle);
            yield return new WaitForSeconds(_shootWaitTime);

            ShootDarts(_dartSpawnPoint1.transform.position, _shootAngle);
            yield return new WaitForSeconds(_shootFrequency);
            yield return null;
        }
        
        yield break;
    }

    private void ShootDarts(Vector3 spawnPosition, Quaternion rotation)
    {
        _lastDartShot = Instantiate(_dartShot, spawnPosition, rotation);
        
        Rigidbody2D thrownRB = _lastDartShot.GetComponent<Rigidbody2D>();
        if (thrownRB != null)
        {
            thrownRB.AddForce(_aimInput * _shootStrenght, ForceMode2D.Impulse);
            //RuntimeManager.PlayOneShot(_throwSFX, transform.position);
        }
    }

    public void SwitchGun()
    {
        _canShoot = !_canShoot;
    }

    public void TiltShooterUp()
    {
        gameObject.transform.rotation = new Quaternion(0, 0, _tiltUpAngle, 0); 
    }

    public void TiltShooterDown()
    {
        gameObject.transform.rotation = new Quaternion(0, 0, _tiltDownAngle, 0);
    }

    public void ResetShootAngle()
    {
        gameObject.transform.rotation = Quaternion.identity;
    }

}
