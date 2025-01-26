using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

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
    protected LayerMask _shaftLayerMaskExcludeWhileFlying;

    [SerializeField]
    protected LayerMask _shaftLayerMaskWhileStuck;

    [Header("Misc")]
    [SerializeField]
    protected float _owningPlayerInvulnerabilityTime = .5f;

    [Header("Impact")]
    [SerializeField]
    EventReference _impactSFX;
    EventInstance _impactInstance;
    
    [SerializeField]
    EventReference _spikeToSpikeSFX;
    
    [SerializeField]
    EventReference _takeDamageSFX;
    EventInstance _takeDamageInstance;

    protected Rigidbody2D _rb;

    protected GameObject _owningPlayerObject;
    protected bool _canHitOwner = false;

    protected bool _isStuck = false;
    protected bool _layerMaskChangedToStuck = false;

    protected bool _isFlipped = false;

    protected Vector2 _velocity;

    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _frontSpikeHitBox.OnCollisionEnterEvent.AddListener(OnSpikeCollision);
        _backSpikeHitBox.OnCollisionEnterEvent.AddListener(OnSpikeCollision);
        _shaftHitBox.OnCollisionEnterEvent.AddListener(OnShaftCollision);

        _frontSpikeHitBox.ChangeIncludeLayerMask(_frontSpikeLayerMaskWhileFlying);
        _backSpikeHitBox.ChangeIncludeLayerMask(_backSpikeLayerMaskWhileFlying);
        _shaftHitBox.ChangeExcludeLayerMask(_shaftLayerMaskExcludeWhileFlying);

        _backSpikeHitBox.gameObject.SetActive(false);

        gameManager = FindAnyObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_owningPlayerInvulnerabilityTime > 0 && _canHitOwner == false)
        {
            _owningPlayerInvulnerabilityTime -= Time.deltaTime;
        }
        else
        {
            _canHitOwner = true;
        }

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

    public void SetOwningPlayerObject(GameObject owner)
    {
        _owningPlayerObject = owner;
    }

    public bool GetIsStuck()
    {
        return _isStuck;
    }

    protected void OnSpikeCollision(Collision2D collision)
    {
        RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

        if (collision.gameObject.CompareTag("Player"))
        {
            RuntimeManager.PlayOneShot(_takeDamageSFX, transform.position); // Play take damage sfx when spike hits player
            if (collision.gameObject == _owningPlayerObject && _canHitOwner == false)
            {
                return;
            }
            else
            {
                // damage logic
                Debug.Log("spike player");

                if (collision.rigidbody.gameObject.TryGetComponent(out Health health))
                {
                    health.TakeDamage(1f);
                }

                DestroyObject();
            }
        }

        if (collision.gameObject.CompareTag("ThrownWeapon"))
        {
            RuntimeManager.PlayOneShot(_spikeToSpikeSFX, transform.position); // Play spike to spike SFX when two thrown weapons collide
            Debug.Log("spike thrown weapon");
            DestroyObject();
            return;
        }


        if (_isStuck == false)
        {
            _isStuck = true;

            _frontSpikeHitBox.gameObject.SetActive(false);
            _backSpikeHitBox.gameObject.SetActive(true);

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;

            if (_layerMaskChangedToStuck == false)
            {
                _frontSpikeHitBox.ChangeIncludeLayerMask(_frontSpikeLlayerMaskWhileStuck);
                _backSpikeHitBox.ChangeIncludeLayerMask(_backSpikeLayerMaskWhileStuck);
                _shaftHitBox.ChangeIncludeLayerMask(_shaftLayerMaskWhileStuck);
            }

        }
    }

    protected void OnShaftCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ThrownWeapon"))
        {
            // kill this mf
            Debug.Log("shaft");
            DestroyObject();
        }
    }

    protected void DestroyObject()
    {
        Destroy(gameObject);
    }

    protected void AddThrownWeapon()
    {
        gameManager.
    }
}
