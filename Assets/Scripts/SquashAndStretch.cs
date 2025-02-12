using UnityEngine;

public class SquashAndStretch : MonoBehaviour
{
    [SerializeField]
    protected Vector3 _maxSquash;

    [SerializeField]
    protected float _speedForMaxSquash;

    [SerializeField]
    protected Rigidbody2D _rb;

    [SerializeField]
    protected Transform _counterRotationTransform;

    [SerializeField]
    protected float _spinStrength;

    [SerializeField]
    protected float _angularDampening;

    protected Vector2 _velocity;
    protected Vector2 _lastVelocity;
    protected float _velocityDot;

    protected float _customAngle;
    protected float _lastCustomAngle;
    protected float _customAngularVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private void Update()
    {
        if (_counterRotationTransform != null)
        {
            _counterRotationTransform.rotation = Quaternion.identity;
            //_lastCustomAngle = _customAngle;

            ////float Direction = 1f;
            ////if (_customAngularVelocity < 0)
            ////{
            ////    Direction = 1f;
            ////}
            ////else if (_customAngularVelocity >= 0)
            ////{
            ////    Direction = -1f;
            ////}

            //// continue spinning with dampening
            //_customAngle += _customAngularVelocity * (1 - _angularDampening);

            //// add spin in random direction based on change in direction
            //_customAngle += ((Mathf.Abs(1 - _velocityDot) / 2) * (Mathf.Clamp(_rb.linearVelocityX, -1, 1) * _spinStrength));
            ////_customAngle += ((Mathf.Abs(1 - _velocityDot) / 2) * (Mathf.Clamp(_rb.linearVelocityX, -1, 1) * _spinStrength));
            

            //if (_customAngle < 0)
            //{
            //    _customAngle += 360;
            //    _lastCustomAngle += 360;
            //}
            //else if (_customAngle > 360)
            //{
            //    _customAngle -= 360;
            //    _lastCustomAngle -= 360;
            //}

            //_customAngularVelocity = _customAngle - _lastCustomAngle * Time.deltaTime;

            //// apply rotation
            //_counterRotationTransform.rotation = Quaternion.identity * Quaternion.Euler(new Vector3(0, 0, _customAngle));
        }
    }

    private void FixedUpdate()
    {
        _lastVelocity = _velocity;
        _velocity = _rb.linearVelocity;
        _velocityDot = Vector2.Dot(_velocity, _lastVelocity);


        if (_velocity.sqrMagnitude > 0.01f) // Check if the GameObject is moving and rotate it towards its velocity
        {
            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        
        transform.localScale = Vector3.Lerp(Vector3.one, _maxSquash, Mathf.Clamp((_velocity.magnitude / _speedForMaxSquash), 0, 1));

    }
}
