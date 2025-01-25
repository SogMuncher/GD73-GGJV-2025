using Unity.VisualScripting;
using UnityEngine;
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

    [Header("Attacking")]
    [SerializeField]
    protected float _throwStrength;

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
    protected Vector2 _velocity;
    protected bool _isGrounded = false;

    protected RaycastHit2D[] _raycastHitBuffer = new RaycastHit2D[10];
    protected int _rayHitCount;
    protected RaycastHit2D _rayClosestHit;
    protected float _rayClosestHitDistance;
    protected Rigidbody2D _rayClosestValidRigidBody;

    protected PlayerInput _playerInput;
    protected float _moveInput;
    protected Vector2 _aimInput;

    protected Health _health;

    protected void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _playerInput = GetComponent<PlayerInput>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    protected void Update()
    {

    }

    protected void FixedUpdate()
    {
        _velocity = _rb.linearVelocity;

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
}
