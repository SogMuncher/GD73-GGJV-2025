using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Dart : MonoBehaviour 
{
    [Header("Front")]
    [SerializeField]
    protected HitBox _frontDartHitBox;

    [SerializeField]
    protected LayerMask _frontDartExcludeWhileFlying;

    [SerializeField]
    protected LayerMask _frontDartExcludeWhileStuck;

    [Header("Shaft")]
    [SerializeField]
    protected HitBox _bodyHitBox;

    [SerializeField]
    protected LayerMask _bodyExcludeWhileFlying;

    [SerializeField]
    protected LayerMask _bodyExcludeWhileStuck;


    [Header("Impact")]
    [SerializeField]
    protected GameObject _clashParticlePrefab;

    [SerializeField]
    EventReference _impactSFX;
    EventInstance _impactInstance;

    [SerializeField]
    EventReference _spikeToSpikeSFX;

    [SerializeField]
    EventReference _takeDamageSFX;
    EventInstance _takeDamageInstance;

    [Header("Misc")]
    [SerializeField]
    protected GameObject _spriteObject;

    [SerializeField]
    protected Sprite _spriteWhenGrounded;

    [SerializeField]
    protected ParticleSystem _dustParticleSystem;

    [SerializeField]
    public GameObject TrailPrefab;

    [SerializeField]
    public Transform TrailAnchor;
    public GameObject TrailObject;

    protected Rigidbody2D _rb;

    [SerializeField]
    protected bool _isStuck = false;
    protected bool _layerMaskChangedToStuck = false;

    protected bool _isFlipped = false;

    protected Vector2 _velocity;

    protected Vector3 _lastHitLocation;

    protected bool isDestroying = false;

    protected GameManager gameManager;
    protected Dart thisDart;

    //private GameObject[] _players = new GameObject[2];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Awake()
    {
        thisDart = GetComponent<Dart>();
    }

    protected void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _frontDartHitBox.OnTriggerEnterEvent.AddListener(OnSpikeCollision);
        _bodyHitBox.OnCollisionEnterEvent.AddListener(OnShaftCollision);

        _frontDartHitBox.ChangeExcludeLayerMask(_frontDartExcludeWhileFlying);
        _bodyHitBox.ChangeExcludeLayerMask(_bodyExcludeWhileFlying);

        gameManager = FindAnyObjectByType<GameManager>();
        TrailObject = Instantiate(TrailPrefab, TrailAnchor.position, Quaternion.identity, TrailAnchor);

        AddDartInLevel(thisDart);

    }

    // Update is called once per frame
    protected void Update()
    {

    }

    public bool GetIsStuck()
    {
        return _isStuck;
    }

    protected void OnSpikeCollision(Collider2D collision)
    {

        // if the collision was with a teleporter
        if (collision.gameObject.layer == 16 || collision.gameObject.layer == 18)
        {
            return;
        }

        // if the collision was with a player
        if (collision.gameObject.layer == 10)
        {

            if (collision.attachedRigidbody.gameObject.TryGetComponent(out Health health))
            {
                RuntimeManager.PlayOneShot(_takeDamageSFX, transform.position); // Play take damage sfx when spike hits player
                health.TakeDamage(1, transform.position);
            }

            RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

            _lastHitLocation = collision.ClosestPoint(transform.position);
            DestroyObject();
        }

        if (collision.gameObject.layer == 19)
        {
            if (collision.attachedRigidbody.gameObject.TryGetComponent(out Health health))
            {
                RuntimeManager.PlayOneShot(_takeDamageSFX, transform.position); // Play take damage sfx when spike hits player
                health.TakeDamage(1, transform.position);
            }

            RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

            _lastHitLocation = collision.ClosestPoint(transform.position);
            DestroyObject();
        }

        if (collision.gameObject.CompareTag("ThrownWeapon") || collision.gameObject.CompareTag("Dart"))
        {
            RuntimeManager.PlayOneShot(_spikeToSpikeSFX, transform.position); // Play spike to spike SFX when two thrown weapons collide

            RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

            _lastHitLocation = collision.ClosestPoint(transform.position);

            if (collision.attachedRigidbody.gameObject != null && collision.attachedRigidbody.TryGetComponent(out ThrownWeapon otherWeapon))
            {
                otherWeapon.DestroyObject();
            }

            if (collision.attachedRigidbody.gameObject != null && collision.attachedRigidbody.TryGetComponent(out Dart dart))
            {
                dart.DestroyObject();
            }

            DestroyObject();
            return;
        }


        if (collision.gameObject.layer == 3)
        {
            Debug.Log("Collided With Ground!");
            if (_isStuck == false)
            {
                RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

                _isStuck = true;

                //_frontDartHitBox.gameObject.SetActive(false);
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;

                _spriteObject.GetComponent<SpriteRenderer>().sprite = _spriteWhenGrounded;
                _dustParticleSystem.Play();

                if (_layerMaskChangedToStuck == false)
                {
                    _frontDartHitBox.ChangeExcludeLayerMask(_frontDartExcludeWhileStuck);
                    _bodyHitBox.ChangeExcludeLayerMask(_bodyExcludeWhileStuck);
                }
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

        if (collision.gameObject.CompareTag("ThrownWeapon") || collision.gameObject.CompareTag("Dart"))
        {
            // kill this mf
            Debug.Log("shaft");
            _lastHitLocation = collision.GetContact(0).point;

            if (collision.rigidbody.gameObject != null && collision.rigidbody.TryGetComponent(out ThrownWeapon otherWeapon))
            {
                otherWeapon.DestroyObject();
            }

            if (collision.rigidbody.gameObject != null && collision.rigidbody.TryGetComponent(out Dart dart))
            {
                dart.DestroyObject();
            }

            DestroyObject();
        }
    }

    public void DetachParticle()
    {
        if (TrailObject != null)
        {
            TrailObject.transform.parent = null;
            Destroy(TrailObject, TrailObject.GetComponent<ParticleSystem>().main.duration);
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

    protected void AddDartInLevel(Dart dart)
    {
        gameManager.AddDartToList(dart);
    }
}
