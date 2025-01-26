using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class ThrownWeapon : MonoBehaviour
{
    [Header("Front")]
    [SerializeField]
    protected HitBox _frontSpikeHitBox;

    [SerializeField]
    protected LayerMask _frontSpikeLayerMaskWhileFlying;

    [SerializeField]
    protected LayerMask _frontSpikeLlayerMaskWhileStuck;

    [Header("Back")]
    [SerializeField]
    protected HitBox _backSpikeHitBox;

    [SerializeField]
    protected LayerMask _backSpikeLayerMaskWhileFlying;

    [SerializeField]
    protected LayerMask _backSpikeLayerMaskWhileStuck;

    [Header("Shaft")]
    [SerializeField]
    protected HitBox _shaftHitBox;

    [SerializeField]
    protected LayerMask _shaftLayerMaskWhileFlying;

    [SerializeField]
    protected LayerMask _shaftLayerMaskWhileStuck;


    protected Rigidbody2D _rb;

    protected bool _isStuck = false;
    protected bool _layerMaskChangedToStuck = false;

    protected bool _isFlipped = false;

    protected Vector2 _velocity;

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();

        _frontSpikeHitBox.OnCollisionEnterEvent.AddListener(OnSpikeCollision);
        _backSpikeHitBox.OnCollisionEnterEvent.AddListener(OnSpikeCollision);
        _shaftHitBox.OnCollisionEnterEvent.AddListener(OnShaftCollision);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        if (_isStuck == false)
        {
            _velocity = _rb.linearVelocity;

            if (_rb.linearVelocityX < 0 && _isFlipped == false)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * -1f, transform.localScale.z);
                _isFlipped = true;
            }
            else if (_rb.linearVelocityX > 0 && _isFlipped == true)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * -1f, transform.localScale.z);
                _isFlipped = false;
            }


            if (_velocity.sqrMagnitude > 0.01f) // Check if the GameObject is moving
            {
                float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }

        }
    }

    protected void OnSpikeCollision(Collision2D collision)
    {
        if (_isStuck == false)
        {
            _isStuck = true;

            _backSpikeHitBox.gameObject.SetActive(true);

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;

            if (_layerMaskChangedToStuck == false)
            {

            }
        }
    }

    protected void OnShaftCollision(Collision2D collision)
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    }
}
