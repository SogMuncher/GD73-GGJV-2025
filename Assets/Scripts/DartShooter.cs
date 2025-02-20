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
    [SerializeField] private Vector2 _aimInput;
    private Quaternion _shootAngle;
    private float _tiltUpAngle;
    private float _tiltDownAngle;

    private GameManager _gameManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _aimInput = new Vector2(transform.rotation.x, transform.rotation.y);
        _shootAngle = gameObject.transform.rotation;
        _gameManager.RoundStarting.AddListener(CallShootDartsCoroutine);
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
    }

    private IEnumerator ShootDartsCorroutine()
    {
        while (_canShoot)
        {
            ShootDarts(_dartSpawnPoint1.transform.position, _shootAngle);
            yield return new WaitForSeconds(_shootWaitTime);

            ShootDarts(_dartSpawnPoint2.transform.position, _shootAngle);
            yield return new WaitForSeconds(_shootWaitTime);

            ShootDarts(_dartSpawnPoint2.transform.position, _shootAngle);
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
