using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class Follower : MonoBehaviour
{
    [SerializeField]
    protected GameObject _objectToFollow;

    [SerializeField]
    protected Vector3 _anchorPositionOffset;

    [SerializeField]
    protected float _followStrength;

    [SerializeField]
    protected float _followDampStrength;

    protected Rigidbody2D _rb;

    protected Vector2 _velocity;
    //protected float _angularVelocity;

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeFollowObject(GameObject gameObject)
    {
        _objectToFollow = gameObject;
    }

    protected void FixedUpdate()
    {
        _velocity = _rb.linearVelocity;
        Vector2 otherVelocity = Vector2.zero;

        if (_objectToFollow.TryGetComponent(out Rigidbody2D otherRB))
        {
            otherVelocity = otherRB.linearVelocity;
        }

        float relativeVelocity = 1 - Vector2.Dot(_velocity, otherVelocity);
        
        Vector2 goalPosition = _objectToFollow.transform.position + _anchorPositionOffset;
        float offset = (goalPosition - new Vector2(transform.position.x, transform.position.y)).magnitude;

        //float springForce = (offset * _followStrength) - (relativeVelocity * _followDampStrength);
        float springForce = (offset * _followStrength) - _followDampStrength;

        _rb.AddForce((goalPosition - new Vector2(transform.position.x, transform.position.y)).normalized * springForce);
    }
}
