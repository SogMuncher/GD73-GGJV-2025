using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody2D))]
public class ThrownWeapon : MonoBehaviour
{
    [Header("Front")]
    [SerializeField]
    protected HitBox _frontSpikeHitBox;

    [SerializeField]
    protected LayerMask _frontSpikeExcludeWhileFlying;

    [SerializeField]
    protected LayerMask _frontSpikeExcludeWhileStuck;

    [Header("Back")]
    [SerializeField]
    protected HitBox _backSpikeHitBox;

    [SerializeField]
    protected LayerMask _backSpikeExcludeWhileFlying;

    [SerializeField]
    protected LayerMask _backSpikeExcludeWhileStuck;

    [Header("Shaft")]
    [SerializeField]
    protected HitBox _shaftHitBox;

    [SerializeField]
    protected LayerMask _shaftExcludeWhileFlying;

    [SerializeField]
    protected LayerMask _shaftExcludeWhileStuck;

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

    [SerializeField]
    public GameObject _trailPrefab;
    [SerializeField]
    public Transform _trailAnchor;
    public GameObject _trailObject;

    [SerializeField]
    protected GameObject _clashParticlePrefab;

    protected Rigidbody2D _rb;

    protected GameObject _owningPlayerObject;
    protected bool _canHitOwner = false;

    protected bool _isStuck = false;
    protected bool _layerMaskChangedToStuck = false;

    protected bool _isFlipped = false;

    protected Vector2 _velocity;

    protected Vector3 _lastHitLocation;

    protected bool isDestroying = false;

    private GameManager gameManager;
    public ThrownWeapon thisWeapon;

    [SerializeField] private GameObject _flickeringSprite;
    //private PlayerController[] _players;
    //[SerializeField] private float _detectionRange = 3f;
    //private List<PlayerController> _players;
    //private float _distanceToPlayer;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        thisWeapon = GetComponent<ThrownWeapon>();
        //_players.Add(FindObjectOfType<PlayerController>());
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _frontSpikeHitBox.OnTriggerEnterEvent.AddListener(OnSpikeCollision);
        _backSpikeHitBox.OnTriggerEnterEvent.AddListener(OnSpikeCollision);
        _shaftHitBox.OnCollisionEnterEvent.AddListener(OnShaftCollision);

        _frontSpikeHitBox.ChangeExcludeLayerMask(_frontSpikeExcludeWhileFlying);
        _backSpikeHitBox.ChangeExcludeLayerMask(_backSpikeExcludeWhileFlying);
        _shaftHitBox.ChangeExcludeLayerMask(_shaftExcludeWhileFlying);

        _backSpikeHitBox.gameObject.SetActive(false);

        gameManager = FindAnyObjectByType<GameManager>();
        _trailObject = Instantiate(_trailPrefab, _trailAnchor.position, Quaternion.identity, _trailAnchor);

        AddThrownWeapon(thisWeapon);

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

        //foreach (PlayerController player in _players)
        //{
        //    if (_players != null)
        //    {
        //        float distance = Vector3.Distance(this.transform.position, player.transform.position);
        //        _distanceToPlayer = distance;
        //    }
        //}

        //if (_distanceToPlayer <= _detectionRange)
        //{
        //    _flickeringSprite.SetActive(true);
        //}
        //else
        //{
        //    _flickeringSprite.SetActive(false);
        //}
    }

    public void SetOwningPlayerObject(GameObject owner)
    {
        _owningPlayerObject = owner;
    }

    public bool GetIsStuck()
    {
        return _isStuck;
    }

    protected void OnSpikeCollision(Collider2D collision)
    {

        // if the collision was with a teleporter
        if (collision.gameObject.layer == 16)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            
            if (collision.gameObject == _owningPlayerObject && _canHitOwner == false)
            {
                return;
            }
            else
            {
                // damage logic
                Debug.Log("spike player");

                if (collision.attachedRigidbody.gameObject.TryGetComponent(out Health health))
                {
                    RuntimeManager.PlayOneShot(_takeDamageSFX, transform.position); // Play take damage sfx when spike hits player
                    health.TakeDamage(1, transform.position);
                }

                RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

                _lastHitLocation = collision.ClosestPoint(transform.position);
                DestroyObject();
            }
        }

        if (collision.gameObject.CompareTag("ThrownWeapon"))
        {
            RuntimeManager.PlayOneShot(_spikeToSpikeSFX, transform.position); // Play spike to spike SFX when two thrown weapons collide

            RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

            Debug.Log("spike thrown weapon");
            _lastHitLocation = collision.ClosestPoint(transform.position);

            if (collision.attachedRigidbody.gameObject != null && collision.attachedRigidbody.TryGetComponent(out ThrownWeapon otherWeapon))
            {
                otherWeapon.DestroyObject();
            }

            DestroyObject();
            return;
        }

        if (_isStuck == false)
        {
            RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

            _isStuck = true;

            _frontSpikeHitBox.gameObject.SetActive(false);
            _backSpikeHitBox.gameObject.SetActive(true);
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;

            if (_layerMaskChangedToStuck == false)
            {
                _frontSpikeHitBox.ChangeExcludeLayerMask(_frontSpikeExcludeWhileStuck);
                _backSpikeHitBox.ChangeExcludeLayerMask(_backSpikeExcludeWhileStuck);
                _shaftHitBox.ChangeExcludeLayerMask(_shaftExcludeWhileStuck);
            }

        }
    }

    protected void OnShaftCollision(Collision2D collision)
    {
        // if the collision was with a teleporter
        if (collision.gameObject.layer == 16)
        {
            return;
        }

        if (collision.gameObject.CompareTag("ThrownWeapon"))
        {
            // kill this mf
            Debug.Log("shaft");
            _lastHitLocation = collision.GetContact(0).point;

            if (collision.rigidbody.gameObject != null && collision.rigidbody.TryGetComponent(out ThrownWeapon otherWeapon))
            {
                otherWeapon.DestroyObject();
            }

            DestroyObject();
        }
    }

    public void DetachParticle()
    {
        if (_trailObject != null)
        {
            _trailObject.transform.parent = null;
            Destroy(_trailObject, _trailObject.GetComponent<ParticleSystem>().main.duration);
        }
    }

    public void DestroyObject()
    {
        if (isDestroying == true)
        {
            return;
        }

        isDestroying = true;

        DetachParticle();

        if (_lastHitLocation == Vector3.zero)
        {
            _lastHitLocation = transform.position;
        }

        Instantiate(_clashParticlePrefab, _lastHitLocation, Quaternion.identity);

        Destroy(gameObject);
    }

    protected void AddThrownWeapon(ThrownWeapon weapon)
    {
        gameManager.AddWeaponsToList(weapon);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out SpearDetector bubble))
        {
            //_players.Add(bubble);
            _flickeringSprite.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out SpearDetector bubble))
        {
            _flickeringSprite.SetActive(false);
        }
    }
}
