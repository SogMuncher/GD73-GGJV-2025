using UnityEngine;

public class HVTeleporter : MonoBehaviour
{
    [Header("IS VERTICAL?")]
    [SerializeField] private bool _isVertical = true;

    [SerializeField] private Transform _tpExit;
    [SerializeField] private int _maxDistance = 10;
    [SerializeField] protected LayerMask _rayLayerMask;
    private ThrownWeapon _lastWeapon;
    [SerializeField] private int _maxSpearCollisionsWithTP = 1;

    [HideInInspector]
    public GameObject _lastObjectTeleported;
    [SerializeField] private EjectDirection _direction;
    private enum EjectDirection
    {
        up,
        down,
        left,
        right,
    }

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        GetHits();

        OnDrawGizmos();
    }
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    // if the object is a thrown weapon
    //    bool isWeapon = false;
    //    ThrownWeapon weapon = null;
    //    if (collision.gameObject.layer == 15)
    //    {
    //        // if the object is the same as the last one that entered this passage, do not teleport it
    //        if (collision.attachedRigidbody.gameObject == _lastObjectTeleported)
    //        {
    //            return;
    //        }

    //        isWeapon = true;
    //        weapon = collision.attachedRigidbody.GetComponent<ThrownWeapon>();

    //        weapon.DetachParticle();
    //    }

    //    if (collision.gameObject.layer != 3)
    //    {
    //        _lastObjectTeleported = collision.attachedRigidbody.gameObject;
    //        //_connector.GetComponent<Passages>()._lastObjectTeleported = _lastObjectTeleported;

    //        // Get the collision object's initial X position
    //        float collisionInitialY = collision.gameObject.transform.position.y;
    //        Debug.Log(collisionInitialY);
    //        Debug.Log("Collider" + collision.gameObject.name);
    //        // Reposition the collision object
    //        Vector3 newPosition = new Vector3(_connector.position.x, collisionInitialY, _connector.position.z);
    //        newPosition.y = collisionInitialY; // Maintain the collision object's initial Y
    //        collision.attachedRigidbody.gameObject.transform.position = newPosition;


    //    }

    //    if (isWeapon)
    //    {
    //        weapon.TrailObject = Instantiate(weapon.TrailPrefab, weapon.TrailAnchor.position, Quaternion.identity, weapon.TrailAnchor);
    //    }
    //}

    public void GetHits()
    {
        if (_isVertical)
        {

            //int raycastHits = Physics2D.RaycastNonAlloc(_bottomOfTrigger.position, Vector3.up,_raycastHitBuffer, _maxDistance, _rayLayerMask );
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, _maxDistance, _rayLayerMask);


            //if the object is a thrown weapon
            bool isWeapon = false;
            if (!hit) return;
            if (hit.collider.gameObject.layer == 15)
            {

                // if the object is the same as the last one that entered this passage, do not teleport it
                if (hit.collider.attachedRigidbody.gameObject == _lastObjectTeleported)
                {
                    return;
                }

                isWeapon = true;
                _lastWeapon = hit.collider.attachedRigidbody.GetComponent<ThrownWeapon>();
                _lastWeapon.TimesCollidedWithTeleporter++;
                


                _lastWeapon.DetachParticle();
            }
            if (hit.collider.gameObject.layer != 3)
            {
                if (hit.collider.gameObject.layer == 15 && _lastWeapon.TimesCollidedWithTeleporter > _maxSpearCollisionsWithTP)
                {
                    return ;
                }
                float distance = Mathf.Abs(hit.point.x - transform.position.x);
                Debug.Log(distance);

                switch (_direction)
                {
                    

                    case EjectDirection.left:
                        hit.collider.attachedRigidbody.gameObject.transform.position = new Vector2(_tpExit.position.x, _tpExit.position.y + distance);
                        
                        hit.collider.attachedRigidbody.linearVelocity = new Vector2(hit.collider.attachedRigidbody.linearVelocityY, -Mathf.Abs(hit.collider.attachedRigidbody.linearVelocityX));
                        break;

                    case EjectDirection.right:
                        hit.collider.attachedRigidbody.gameObject.transform.position = new Vector2(_tpExit.position.x , _tpExit.position.y + distance);
                        hit.collider.attachedRigidbody.linearVelocity = new Vector2(hit.collider.attachedRigidbody.linearVelocityY, Mathf.Abs(hit.collider.attachedRigidbody.linearVelocityX));
                        break;
                }


                if (isWeapon)
                {
                    _lastWeapon.TrailObject = Instantiate(_lastWeapon.TrailPrefab, _lastWeapon.TrailAnchor.position, Quaternion.identity, _lastWeapon.TrailAnchor);
                    _lastWeapon.ChargedParticleObject = Instantiate(_lastWeapon.ChargedParticlePrefab, _lastWeapon.transform.position, _lastWeapon.transform.rotation, _lastWeapon.transform);
                }


            }
        }

        if (!_isVertical)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, _maxDistance, _rayLayerMask);


            //if the object is a thrown weapon
            bool isWeapon = false;
            if (!hit) return;
            if (hit.collider.gameObject.layer == 15)
            {

                // if the object is the same as the last one that entered this passage, do not teleport it
                if (hit.collider.attachedRigidbody.gameObject == _lastObjectTeleported)
                {
                    return;
                }

                isWeapon = true;
                _lastWeapon = hit.collider.attachedRigidbody.GetComponent<ThrownWeapon>();
                _lastWeapon.TimesCollidedWithTeleporter++;



                _lastWeapon.DetachParticle();
            }
            if (hit.collider.gameObject.layer != 3)
            {

                if (hit.collider.gameObject.layer == 15 && _lastWeapon.TimesCollidedWithTeleporter > _maxSpearCollisionsWithTP)
                {
                    return ;
                }
                float distance = Mathf.Abs(hit.point.y - transform.position.y);
                Debug.Log(distance);

                switch (_direction)
                {
                    case EjectDirection.up:

                        hit.collider.attachedRigidbody.gameObject.transform.position = new Vector2(_tpExit.position.x + distance, _tpExit.position.y);
                        hit.collider.attachedRigidbody.linearVelocity = new Vector2(hit.collider.attachedRigidbody.linearVelocityY, Mathf.Abs(hit.collider.attachedRigidbody.linearVelocityX));
                        break;

                    case EjectDirection.down:

                        hit.collider.attachedRigidbody.gameObject.transform.position = new Vector2(_tpExit.position.x + distance, _tpExit.position.y);
                        hit.collider.attachedRigidbody.linearVelocity = new Vector2(hit.collider.attachedRigidbody.linearVelocityY, -Mathf.Abs(hit.collider.attachedRigidbody.linearVelocityX));
                        break;
                }
                //  hit.collider.gameObject.transform.position = new Vector2(_tpExit.position.x + distance, _tpExit.position.y );

                if (isWeapon)
                {
                    _lastWeapon.TrailObject = Instantiate(_lastWeapon.TrailPrefab, _lastWeapon.TrailAnchor.position, Quaternion.identity, _lastWeapon.TrailAnchor);
                    _lastWeapon.ChargedParticleObject = Instantiate(_lastWeapon.ChargedParticlePrefab, _lastWeapon.transform.position, _lastWeapon.transform.rotation, _lastWeapon.transform);
                }

            }
        }


    }

    private void OnDrawGizmos()
    {
        if (_isVertical)
        {
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, _maxDistance, _rayLayerMask);

            Debug.DrawRay(transform.position, Vector2.right * 10, Color.red);

        }
        if (!_isVertical)
        {
            Debug.DrawRay(transform.position, Vector2.up * 10, Color.red);

        }
    }

}
