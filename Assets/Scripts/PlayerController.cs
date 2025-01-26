using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput), typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Locomotion")]
    [SerializeField]
    protected float _maxSpeed;

    [SerializeField]
    protected float _accelerationSpeed;

    [SerializeField]
    protected float _jumpStrength;

    [SerializeField]
    protected float _jumpCost;

    [SerializeField]
    protected float _airControl;

    [SerializeField]
    protected float _playerBounceForce = 5f;

    [SerializeField]
    EventReference _bounceSFX;

    [SerializeField]
    EventReference _jumpSFX;
    EventInstance _jumpSFXInstance;

    [Header("Attacking")]
    [SerializeField]
    protected GameObject _weapon;

    [SerializeField]
    protected int _maxAmmo = 3;

    [SerializeField]
    protected float _reloadTime = 2f;

    [SerializeField]
    protected float _throwCooldownTime = .25f;

    [SerializeField]
    protected float _throwStrength;

    [SerializeField]
    protected float _aimSpeed;

    [SerializeField]
    protected GameObject _thrownWeaponPrefab;

    [SerializeField]
    protected Transform _thrownWeaponSpawnPoint;

    //[Header("Aim Spring Settings")]
    //[SerializeField]
    //protected float _aimSpringStrength;

    //[SerializeField]
    //protected float _aimSpringDampStrength;

    [SerializeField]  
    EventReference _throwSFX;
    EventInstance _throwSFXInstance;

    [SerializeField]
    EventReference _reloadSFX;

    [Header("Spring Settings")]
    [SerializeField]
    protected float _rayMaxDistance;

    [SerializeField]
    protected LayerMask _rayLayerMask;

    [SerializeField]
    protected float _springRestHeight;

    [SerializeField]
    protected float _springStrength;

    [SerializeField]
    protected float _springDampStrength;

    [Header("Sticky Settings")]
    [SerializeField]
    private bool _isSticky;

    [SerializeField]
    private InputAction _stick;

    protected Rigidbody2D _rb;
    //protected Rigidbody2D _weaponRB;
    protected Vector2 _velocity;
    //protected float _weaponAngularVelocity;
    protected bool _isGrounded = false;

    protected RaycastHit2D[] _raycastHitBuffer = new RaycastHit2D[10];
    protected int _rayHitCount;
    protected RaycastHit2D _rayClosestHit;
    protected float _rayClosestHitDistance;
    protected Rigidbody2D _rayClosestValidRigidBody;

    protected PlayerInput _playerInput;
    protected float _moveInput;
    protected Vector2 _aimInput;
    protected float _weaponRotationY;

    protected GameObject _lastThrownWeapon;

    protected int _currentAmmo = 3;
    protected bool _isReloading;
    protected bool _throwIsOnCooldown;

    protected Health _health;

    public UnityEvent OnPaused;

    private Vector3 _startLocation;

  



    protected void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _playerInput = GetComponent<PlayerInput>();
        //_weaponRB = _weapon.GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _health.OnDamagedEvent.AddListener(OnDamaged);
        _startLocation = transform.position;
    }

    // Update is called once per frame
    protected void Update()
    {
        Vector3 goalRotation = new Vector3(0, _weaponRotationY, 90 * _aimInput.y);
        Vector3 currentRotation = _weapon.transform.rotation.eulerAngles;
        // so values dont need to be huge in the inspector
        float aimSpeed = _aimSpeed * 10;

        float rotationY = Mathf.Lerp(currentRotation.y, goalRotation.y, aimSpeed * Time.deltaTime);
        float rotationZ = Mathf.Lerp(currentRotation.z, goalRotation.z, aimSpeed * Time.deltaTime);

        _weapon.transform.rotation = Quaternion.Euler(new Vector3(0, rotationY, 90 * _aimInput.y));
        //_weapon.transform.rotation = Quaternion.Euler(new Vector3(0, rotationY, rotationZ));
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            Vector2 direction = ((collision.transform.position) - (this.transform.position)).normalized;

            _rb.AddForce(-direction * _playerBounceForce, ForceMode2D.Impulse);

            RuntimeManager.PlayOneShot(_bounceSFX, transform.position); // Play the bounce off other player sound


        }

        if (collision.gameObject.layer == 3)
        {
            _isSticky = true;
        }

       

        Debug.Log(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            _isSticky = false;
        }

       

       

        Debug.Log(collision);
    }



    protected void FixedUpdate()
    {
        _velocity = _rb.linearVelocity;

        //CalculateAimSpring();

        GroundCheckRay();

        CalculateMovement();

        if (_isGrounded == true)
        {
            GetClosestRayHit();
            CalculateSpringForces();
        }
    }

    protected void CalculateMovement()
    {
        float force = _moveInput * _accelerationSpeed * Time.deltaTime;
        _rb.AddForceX(force);

        // clamp to max speed
        if (_rb.linearVelocityX > _maxSpeed)
        {
            _rb.linearVelocityX = _maxSpeed;
        }
    }

    //protected void CalculateAimSpring()
    //{
    //    //Vector3 goalRotation = new Vector3(0, _weaponRotationY, 90 * _aimInput.y);

    //    //_weapon.transform.rotation = Quaternion.RotateTowards(_weapon.transform.rotation, Quaternion.Euler(goalRotation), Time.deltaTime);
    //    //float offsetY = _weapon.transform.rotation.eulerAngles.y - goalRotation.y;
    //    //float offsetZ = _weapon.transform.rotation.eulerAngles.z - goalRotation.z;

    //    //float springForceY = (offsetY * _aimSpringStrength) - (_weaponRB.angularVelocity - _aimSpringDampStrength);
    //    //float springForceZ = (offsetZ * _aimSpringStrength) - (_weaponRB.angularVelocity - _aimSpringDampStrength);

    //    //_weaponRB.AddTorque(springForceZ);
    //}

    protected void GroundCheckRay()
    {
       _rayHitCount = Physics2D.RaycastNonAlloc(transform.position, Vector2.down, _raycastHitBuffer, _rayMaxDistance, _rayLayerMask);

        if (_rayHitCount > 0 )
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    protected void GetClosestRayHit()
    {
        float dist = _rayMaxDistance;
        int closestHitInt = 0;
        int closestRigidbodyInt = 0;

        for (int i = 0; i < _rayHitCount; i++)
        {
            if (dist > _raycastHitBuffer[i].distance)
            {
                closestHitInt = i;

                dist = _raycastHitBuffer[i].distance;

                if (_raycastHitBuffer[i].rigidbody != null)
                {
                    closestRigidbodyInt = i;
                }
            }
        }

        _rayClosestHit = _raycastHitBuffer[closestHitInt];
        _rayClosestValidRigidBody = _raycastHitBuffer[closestRigidbodyInt].rigidbody;
        _rayClosestHitDistance = dist;
    }

    protected void CalculateSpringForces()
    {
        if (_rayClosestHit.rigidbody != null)
        {
            if (_rayClosestHit.rigidbody.TryGetComponent<ThrownWeapon>(out ThrownWeapon thrownWeapon))
            {
                if (thrownWeapon.GetIsStuck() == false)
                {
                    return;
                }
            }
        }

        Vector2 otherVelocity = Vector2.zero;
        Rigidbody2D hitBody = _rayClosestValidRigidBody;
        if ( hitBody != null )
        {
            otherVelocity = hitBody.linearVelocity;
        }

        // how much our velocity is downwards
        float rayDirVelocity = Vector2.Dot(Vector2.down, _velocity);
        // how much the hit body is moving downwards
        float otherDirVelocity = Vector2.Dot(Vector2.down, otherVelocity);


        float relativeVelocity = rayDirVelocity - otherDirVelocity;

        // where is the ground relative to our desired rest height? 
        float offset = _rayClosestHitDistance - _springRestHeight;

        // calculate dampened spring force
        float springForce = (offset * _springStrength) - (relativeVelocity * _springDampStrength);

        // add force to the rigidbody
        if (springForce > 0f)
        {
            springForce = 0f;
        }
        _rb.AddForce(Vector2.down * springForce);
    }

    protected void OnMovement(InputValue inputValue)
    {
        _moveInput = inputValue.Get<float>();

        Debug.Log($"Player {_playerInput.playerIndex} Move Input: {_moveInput}");
    }

    protected void OnAim(InputValue inputValue)
    {
        _aimInput = inputValue.Get<Vector2>();

        if (_aimInput.x < 0)
        {
            _weaponRotationY = 180f;
        }
        else if (_aimInput.x > 0)
        {
            _weaponRotationY = 0f;
        }

        //_weapon.transform.rotation = Quaternion.Euler(new Vector3(0, _weaponRotationY, 90 * _aimInput.y));

        Debug.Log($"Player {_playerInput.playerIndex} Aim Input: {_aimInput}");
    }

    protected void OnFire()
    {
        if (_currentAmmo > 0 && _throwIsOnCooldown == false)
        {
            _currentAmmo--;

            _lastThrownWeapon = Instantiate(_thrownWeaponPrefab, _thrownWeaponSpawnPoint.position, _weapon.transform.rotation);
            _lastThrownWeapon.GetComponent<ThrownWeapon>().SetOwningPlayerObject(gameObject);

            Rigidbody2D thrownRB = _lastThrownWeapon.GetComponent<Rigidbody2D>();

            if (thrownRB != null)
            {
                thrownRB.AddForce(_aimInput * _throwStrength, ForceMode2D.Impulse);
                RuntimeManager.PlayOneShot(_throwSFX, transform.position);
            }

            _throwIsOnCooldown = true;
            StartCoroutine(ThrowCooldownCoroutine());
        }

        if (_currentAmmo <= 0 && _isReloading == false)
        {
            _isReloading = true;
            StartCoroutine(ReloadTimerCoroutine());
        }

        Debug.Log($"Player {_playerInput.playerIndex} Fired");
    }

    protected void OnJump()
    {
        if (_rb.linearVelocityY < 0)
        {
            _rb.linearVelocityY = 0f;
        }

        _rb.AddForceY(_jumpStrength, ForceMode2D.Impulse);
        RuntimeManager.PlayOneShot(_jumpSFX, transform.position);

        Debug.Log($"Player {_playerInput.playerIndex} Jumped");
    }
    public void OnStick()
    {
       if(_isSticky == true)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezePosition;
            

        } 

        

        

       
    }

    public void OnUnstick()
    {
        if(_isSticky == true)
        {

        }
            _rb.constraints = RigidbodyConstraints2D.None;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected void OnDamaged()
    {
        // logic here for when damaged, bounce??
    }

    protected IEnumerator ReloadTimerCoroutine()
    {
        _isReloading = true;

        _weapon.SetActive(false);

        yield return new WaitForSeconds(_reloadTime);

        _weapon.SetActive(true);
        _currentAmmo = _maxAmmo;
        _isReloading = false;
        RuntimeManager.PlayOneShot(_reloadSFX, transform.position); // Play reload sound when ammo is max ammo


        StartCoroutine(ThrowCooldownCoroutine());
        yield break;
    }

    protected IEnumerator ThrowCooldownCoroutine()
    {
        _throwIsOnCooldown = true;

        float time = 0f;

        while (time < _throwCooldownTime)
        {
            _weapon.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / _throwCooldownTime);

            time += Time.deltaTime;
            yield return null;
        }

        _weapon.transform.localScale = Vector3.one;

        //yield return new WaitForSeconds(_throwCooldownTime);

        _throwIsOnCooldown = false;
        yield break;
    }

    private void OnDrawGizmos()
    {
        if (_isGrounded == true)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y) + (Vector2.down * _rayMaxDistance));
        }
        else if (_isGrounded == false)
        {
             Gizmos.color = Color.green;

            Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y) + (Vector2.down * _rayMaxDistance));
        }
    }

    public void OnPause()
    {
       
            OnPaused.Invoke();

        
    }

    public void ResetTransform()
    {
        transform.position = _startLocation;
    }

    
}
