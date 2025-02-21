using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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
    EventReference _harpoonClashSFX;

    [SerializeField]
    EventReference _takeDamageSFX;
    EventInstance _takeDamageInstance;

    [Header("Misc")]
    [SerializeField]
    private GameObject _spriteObject;

    [SerializeField]
    private Sprite _spriteWhenGrounded;

    [SerializeField]
    private ParticleSystem _dustParticleSystem;

    [SerializeField]
    private GameObject TrailPrefab;

    [SerializeField]
    private Transform TrailAnchor;
    private GameObject TrailObject;

    private Rigidbody2D _rb;

    [SerializeField]
    private bool _layerMaskChangedToStuck = false;

    private bool _isStuck = false;

    private Vector2 _velocity;

    private Vector3 _lastHitLocation;

    private bool isDestroying = false;

    private GameManager _gameManager;
    private Dart _thisDart;

    [SerializeField] private float _lifeTimer = 3.0f;

    //private GameObject[] _players = new GameObject[2];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _thisDart = GetComponent<Dart>();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _frontDartHitBox.OnTriggerEnterEvent.AddListener(OnSpikeCollision);
        _bodyHitBox.OnCollisionEnterEvent.AddListener(OnShaftCollision);

        _frontDartHitBox.ChangeExcludeLayerMask(_frontDartExcludeWhileFlying);
        _bodyHitBox.ChangeExcludeLayerMask(_bodyExcludeWhileFlying);

        _gameManager = FindAnyObjectByType<GameManager>();
        TrailObject = Instantiate(TrailPrefab, TrailAnchor.position, Quaternion.identity, TrailAnchor);

        AddDartInLevel(_thisDart);

        StartCoroutine(DestroySelfCorroutine());
    }

    public bool GetIsStuck()
    {
        return _isStuck;
    }

    private void OnSpikeCollision(Collider2D collision)
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
            RuntimeManager.PlayOneShot(_harpoonClashSFX, transform.position); // Play spike to spike SFX when two thrown weapons collide

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

        if (collision.gameObject.CompareTag("Switch"))
        {
            RuntimeManager.PlayOneShot(_harpoonClashSFX, transform.position); // Play spike to spike SFX when two thrown weapons collide

            RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

            Debug.Log("Switch Hit");
            _lastHitLocation = collision.ClosestPoint(transform.position);

            if (collision.TryGetComponent(out DartShooterSwitch leverSwitch))
            {
                leverSwitch.SwitchOnOff();
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

    private void OnShaftCollision(Collision2D collision)
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

    private void DetachParticle()
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

    private void AddDartInLevel(Dart dart)
    {
        _gameManager.AddDartToList(dart);
    }

    private IEnumerator DestroySelfCorroutine()
    {
        float timer = 0f;
        while (timer < _lifeTimer)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        DestroyObject();
        yield break;
    }
}
