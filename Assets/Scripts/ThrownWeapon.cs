using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening.Core.Easing;
using System.ComponentModel;
using Unity.Jobs;
using Unity.Collections;

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

    [SerializeField] 
    protected GameObject _flickeringSprite;
    protected Color _flickeringSpriteStartColor;

    [SerializeField] 
    protected float _detectionRange = 4f;

    [SerializeField]
    protected AnimationCurve _flickeringColorLerpAlpha;
    protected GameObject _player1;
    protected GameObject _player2;

    protected Rigidbody2D _rb;

    protected GameObject _owningPlayerObject;
    protected bool _canHitOwner = false;

    protected bool _isStuck = false;
    protected bool _layerMaskChangedToStuck = false;

    protected bool _isFlipped = false;

    protected Vector2 _velocity;

    protected Vector3 _lastHitLocation;

    protected bool isDestroying = false;

    protected GameManager _gameManager;
    protected ThrownWeapon _thisWeapon;

    [SerializeField, Unity.Collections.ReadOnly]
    public int TimesCollidedWithTeleporter = 0;

    //private GameObject[] _players = new GameObject[2];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Awake()
    {
        _thisWeapon = GetComponent<ThrownWeapon>();
    }

    protected void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _frontSpikeHitBox.OnTriggerEnterEvent.AddListener(OnSpikeCollision);
        _backSpikeHitBox.OnTriggerEnterEvent.AddListener(OnSpikeCollision);
        _shaftHitBox.OnCollisionEnterEvent.AddListener(OnShaftCollision);

        _frontSpikeHitBox.ChangeExcludeLayerMask(_frontSpikeExcludeWhileFlying);
        _backSpikeHitBox.ChangeExcludeLayerMask(_backSpikeExcludeWhileFlying);
        _shaftHitBox.ChangeExcludeLayerMask(_shaftExcludeWhileFlying);

        _backSpikeHitBox.gameObject.SetActive(false);

        _gameManager = FindAnyObjectByType<GameManager>();

        TrailObject = Instantiate(TrailPrefab, TrailAnchor.position, Quaternion.identity, TrailAnchor);

        _flickeringSpriteStartColor = _flickeringSprite.GetComponent<SpriteRenderer>().color;

        if (_gameManager != null)
        {
            AddThrownWeapon(_thisWeapon);
        }
        

        //for (int i = 0; i < _players.Length; i++)
        //{
        //    _players[i] = GameObject.FindGameObjectWithTag("SpearDetector");
        //}

        _player1 = GameObject.FindGameObjectWithTag("Player1");
        _player2 = GameObject.FindGameObjectWithTag("Player2");
   }

    // Update is called once per frame
    protected void Update()
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

        //foreach (GameObject player in _players)
        //{
        //    if (player != null)
        //    {
        //        float distance = Vector3.Distance(this.transform.position, player.transform.position);

        //        if (distance <= _detectionRange)
        //        {
        //            _flickeringSprite.SetActive(true);
        //        }
        //        else
        //        {
        //            _flickeringSprite.SetActive(false);
        //        }
        //    }
        //}

        float distanceToP1 = Vector3.Distance(_flickeringSprite.transform.position, _player1.transform.position);
        float distanceToP2 = Vector3.Distance(_flickeringSprite.transform.position, _player2.transform.position);

        float closestPlayerDistance = Mathf.Min(distanceToP1, distanceToP2);

        if (closestPlayerDistance <= _detectionRange)
        {
            _flickeringSprite.SetActive(true);
            Color currentColor = _flickeringSprite.GetComponent<SpriteRenderer>().color;
            float lerpedDistance = Mathf.Lerp(1, 0, closestPlayerDistance / _detectionRange);
            float lerpedScale = Mathf.Clamp(Mathf.Lerp(1.0625f, 1.2f, _flickeringSprite.GetComponent<Animator>().GetFloat("ScaleAlpha") * lerpedDistance), 1, 2);
            float alphaOverDistance = _flickeringSprite.GetComponent<Animator>().GetFloat("ColourAlpha") * lerpedDistance;

            _flickeringSprite.GetComponent<Animator>().SetFloat("FlickerSpeed", Mathf.Lerp(.6f, .025f, closestPlayerDistance / _detectionRange));
            _flickeringSprite.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(_flickeringSpriteStartColor.r, _flickeringSpriteStartColor.g, _flickeringSpriteStartColor.b, alphaOverDistance), new Color(1, 0, 0, alphaOverDistance), lerpedDistance);
            _flickeringSprite.transform.localScale = new Vector3(lerpedScale, lerpedScale, 1);
        }
        else
        {
            _flickeringSprite.SetActive(false);
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

    protected void OnSpikeCollision(Collider2D collision)
    {

        if (collision.gameObject.layer == 18)
        {
            return;
        }
         
        // if the collision was with a teleporter
        if (collision.gameObject.layer == 16 )
        {
            return;
        }

        // if the collision was with a player
        if (collision.gameObject.layer == 10)
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

        if (collision.gameObject.layer == 19)
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

        if (collision.gameObject.CompareTag("Switch"))
        {
            RuntimeManager.PlayOneShot(_spikeToSpikeSFX, transform.position); // Play spike to spike SFX when two thrown weapons collide

            RuntimeManager.PlayOneShot(_impactSFX, transform.position); // Play impact sound on collision

            Debug.Log("Switch Hit");
            _lastHitLocation = collision.ClosestPoint(transform.position);

            collision.GetComponentInParent<DartShooterSwitch>().SwitchOnOff();
            
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

            _spriteObject.GetComponent<SpriteRenderer>().sprite = _spriteWhenGrounded;
            _dustParticleSystem.Play();

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

    protected void AddThrownWeapon(ThrownWeapon weapon)
    {
        _gameManager.AddWeaponsToList(weapon);
    }

    //protected void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.TryGetComponent(out SpearDetector bubble))
    //    {
    //        //_players.Add(bubble);
    //        _flickeringSprite.SetActive(true);
    //    }
    //}

    //protected void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.TryGetComponent(out SpearDetector bubble))
    //    {
    //        _flickeringSprite.SetActive(false);
    //    }
    //}
}
