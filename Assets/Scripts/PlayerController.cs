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
    protected float _playerBounceForce = 5f;

    [SerializeField]
    EventReference _bounceSFX;

    [SerializeField]
    EventReference _jumpSFX;
    EventInstance _jumpSFXInstance;

    [Header("Attacking")]
    [SerializeField]
    protected GameObject _weaponVisual;

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

    [SerializeField]
    protected Follower _weaponFollowerScript;

    [SerializeField]
    protected GameObject _weaponFollowerDefaultPosition;

    [SerializeField]
    protected GameObject _weaponFollowerChargePosition;

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

    [Header("Misc")]
    [SerializeField]
    protected float _hitFreezeTime = .25f;

    [SerializeField]
    protected float _hitShakeStrength = 1f;

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
    protected Vector2 _lastValidAimInput;
    protected Vector3 _lastValidMouseViewportPoint;
    protected float _weaponRotationY;

    protected GameObject _lastThrownWeapon;
    protected Vector3 _lastWeaponCollisionPosition;

    protected int _currentAmmo = 3;
    protected bool _isPressingFire = false;
    protected bool _isReloading;
    protected bool _throwIsOnCooldown;

    protected Health _health;

    public UnityEvent OnPaused;
    public UnityEvent OnPauseStopped;
    private bool _isPaused;

    private Vector3 _startLocation;

    protected void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _playerInput = GetComponent<PlayerInput>();
        //_weaponRB = _weapon.GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        _health.OnDamagedEvent.AddListener(OnDamaged);
        _health.OnDamagedEvent.AddListener(StartKnockback);
        _startLocation = transform.position;
        _isPaused = false;
    }

    // Update is called once per frame
    protected void Update()
    {
        //if (_playerInput.currentControlScheme == "Mouse And Keyboard")
        //{
        //    Vector3 playerPositionInViewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        //    // convert to vector2
        //    Vector2 playerPos = new Vector2(playerPositionInViewportPoint.x, playerPositionInViewportPoint.y);
        //    Vector2 mousePos = new Vector2(_lastValidMouseViewportPoint.x, _lastValidMouseViewportPoint.y);

        //    Vector2 dirFromPlayerToMouseBeforeNormalization = mousePos - playerPos;
        //    Vector2 dirFromPlayerToMouse = ((mousePos) - (playerPos)).normalized;

        //    _aimInput = dirFromPlayerToMouse;
        //    _lastValidAimInput = _aimInput;
        //}

        bool weaponIsFlipped = _weaponRotationY == 180;
        Quaternion zRotation = Quaternion.LookRotation(weaponIsFlipped? Vector3.back:Vector3.forward, new Vector3(_aimInput.x, _aimInput.y, 0));
        Quaternion goalRotation = Quaternion.Euler(new Vector3(0, _weaponRotationY, zRotation.eulerAngles.z + 90));
        //Vector3 goalRotation = new Vector3(0, _weaponRotationY, 90 * _aimInput.y);
        Vector3 currentRotation = _weaponVisual.transform.rotation.eulerAngles;
        // so values dont need to be huge in the inspector
        float aimSpeed = _aimSpeed * 10;

        Quaternion lerpedRotation = Quaternion.Slerp(_weaponVisual.transform.rotation, goalRotation, aimSpeed * Time.unscaledDeltaTime);

        float rotationY = Mathf.Lerp(currentRotation.y, goalRotation.y, aimSpeed * Time.deltaTime);
        float rotationZ = Mathf.Lerp(currentRotation.z, goalRotation.z, aimSpeed * Time.deltaTime);

        //_weapon.transform.rotation = Quaternion.Euler(new Vector3(0, rotationY, 90 * _aimInput.y));
        //_weapon.transform.rotation = Quaternion.Euler(new Vector3(0, rotationY, rotationZ + 90));
        _weaponVisual.transform.rotation = lerpedRotation;
    }
    protected void OnCollisionEnter2D(Collision2D collision)
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

        //if (collision.gameObject.layer == 15)
        //{
        //    _lastWeaponCollisionPosition = collision.transform.position;

        //    if (_health.CanBeHurt == false)
        //    {
        //        return;
        //    }

        //}

        Debug.Log(collision);
    }

    public void StartKnockback(Vector3 position)
    {
        _lastWeaponCollisionPosition = position;

        StartCoroutine(DamageOnKnockbackCoroutine());
    }

    protected void OnCollisionExit2D(Collision2D collision)
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
        if (_playerInput.currentControlScheme == "Mouse And Keyboard")
        {
            // convert positions to viewport point (does not care about resolution)
            Vector3 playerPositionInViewportPoint = Camera.main.WorldToViewportPoint(transform.position);
            Vector3 mousePositionInViewportPoint = Camera.main.ScreenToViewportPoint(inputValue.Get<Vector2>());

            _lastValidMouseViewportPoint = mousePositionInViewportPoint;

            // convert to vector2
            Vector2 playerPos = new Vector2(playerPositionInViewportPoint.x, playerPositionInViewportPoint.y);
            Vector2 mousePos = new Vector2(mousePositionInViewportPoint.x, mousePositionInViewportPoint.y);

            Vector2 dirFromPlayerToMouseBeforeNormalization = mousePos - playerPos;
            Vector2 dirFromPlayerToMouse = ((mousePos) - (playerPos)).normalized;

            _aimInput = dirFromPlayerToMouse;
            _lastValidAimInput = _aimInput;

            //Debug.Log($"Player Viewport: {playerPositionInViewportPoint}, Mouse Viewport: {mousePositionInViewportPoint}, Player Vector2: {playerPos}, Mouse Vector2: {mousePos},Direction Before Normalization: {dirFromPlayerToMouseBeforeNormalization}, Direction: {dirFromPlayerToMouse}");
        }
        else if (_playerInput.currentControlScheme == "Generic Controller Scheme")
        {
            if (inputValue.Get<Vector2>() != Vector2.zero)
            {
                _aimInput = inputValue.Get<Vector2>();
                _lastValidAimInput = _aimInput;
            }
            else
            {
                _aimInput = _lastValidAimInput;
            }
        }

        if (_aimInput.x < 0)
        {
            _weaponRotationY = 180f;
        }
        else if (_aimInput.x > 0)
        {
            _weaponRotationY = 0f;
        }

        //_weapon.transform.rotation = Quaternion.Euler(new Vector3(0, _weaponRotationY, 90 * _aimInput.y));

        //Debug.Log($"Player {_playerInput.playerIndex} Aim Input: {_aimInput}");
    }

    protected void OnFire()
    {
        _isPressingFire = !_isPressingFire;

        // the player is holding the fire button while being able to fire
        if (_currentAmmo > 0 && _throwIsOnCooldown == false && _isPressingFire == true)
        {
            _weaponFollowerScript.ChangeFollowObject(_weaponFollowerChargePosition);
        }

        // the player has let go of the fire button while being able to fire
        else if (_currentAmmo > 0 && _throwIsOnCooldown == false && _isPressingFire == false)
        {
            _weaponFollowerScript.ChangeFollowObject(_weaponFollowerDefaultPosition);

            _currentAmmo--;

            _lastThrownWeapon = Instantiate(_thrownWeaponPrefab, _thrownWeaponSpawnPoint.position, _weaponVisual.transform.rotation);

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
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

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

    protected void OnDamaged(Vector3 position)
    {
        // logic here for when damaged, bounce??
    }

    protected IEnumerator ReloadTimerCoroutine()
    {
        _isReloading = true;

        _weaponVisual.SetActive(false);

        yield return new WaitForSeconds(_reloadTime);

        _weaponVisual.SetActive(true);
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
            _weaponVisual.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / _throwCooldownTime);

            time += Time.deltaTime;
            yield return null;
        }

        _weaponVisual.transform.localScale = Vector3.one;

        //yield return new WaitForSeconds(_throwCooldownTime);

        _throwIsOnCooldown = false;
        yield break;
    }

    protected IEnumerator DamageOnKnockbackCoroutine()
    {
        if (_health.GetCurrentHealth() == 0)
        {
            _rb.constraints = RigidbodyConstraints2D.None;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            ResetTransform();
            yield break;
        }

        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Vector3 startingPosition = transform.position;

        float time = 0f;
        while (time < _hitFreezeTime)
        {
            transform.position = startingPosition + new Vector3(Random.Range(-_hitShakeStrength, _hitShakeStrength), Random.Range(-_hitShakeStrength, _hitShakeStrength), 0);

            if (_health.GetCurrentHealth() == 0 || _health.GetCurrentHealth() == _health.MaxHealth)
            {
                _rb.constraints = RigidbodyConstraints2D.None;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                ResetTransform();
                yield break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        _rb.constraints = RigidbodyConstraints2D.None;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (_health.GetCurrentHealth() != _health.MaxHealth)
        {
            transform.position = startingPosition;

            Vector2 direction = ((_lastWeaponCollisionPosition) - (this.transform.position)).normalized;

            _rb.AddForce(-direction * _playerBounceForce, ForceMode2D.Impulse);
        }

        yield break;
    }

    protected void OnDrawGizmos()
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
        if (!_isPaused)
        {
            OnPaused.Invoke();
        }

        if (_isPaused)
        {
            OnPauseStopped.Invoke();
        }

        _isPaused = !_isPaused;
    }

    public void SwitchIsPaused()
    {
        _isPaused = !_isPaused;
    }


    public void ResetTransform()
    {
        transform.position = _startLocation;
        _rb.linearVelocity = Vector2.zero;
    }

    
}
