using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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

    [Header("Attacking")]
    [SerializeField]
    protected GameObject _weapon;

    [SerializeField]
    protected float _throwStrength;

    [SerializeField]
    protected float _aimSpeed;

    //[Header("Aim Spring Settings")]
    //[SerializeField]
    //protected float _aimSpringStrength;

    //[SerializeField]
    //protected float _aimSpringDampStrength;

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

    protected Health _health;

    public UnityEvent OnPaused;

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

            
        }
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
        Debug.Log($"Player {_playerInput.playerIndex} Fired");
    }

    protected void OnJump()
    {
        if (_rb.linearVelocityY < 0)
        {
            _rb.linearVelocityY = 0f;
        }

        _rb.AddForceY(_jumpStrength, ForceMode2D.Impulse);

        Debug.Log($"Player {_playerInput.playerIndex} Jumped");
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
}
