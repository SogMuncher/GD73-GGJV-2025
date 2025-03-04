using UnityEngine;

public class AsymetricalPassages : MonoBehaviour
{
    [SerializeField] private Transform _tpExit;

    private float _perimeterOfTrigger;
    protected RaycastHit2D[] _raycastHitBuffer = new RaycastHit2D[10];
    [SerializeField] private int _maxDistance = 10;
    [SerializeField] protected LayerMask _rayLayerMask;

    [HideInInspector]
    public GameObject _lastObjectTeleported;

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
        //int raycastHits = Physics2D.RaycastNonAlloc(_bottomOfTrigger.position, Vector3.up,_raycastHitBuffer, _maxDistance, _rayLayerMask );
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, _maxDistance, _rayLayerMask);


        //if the object is a thrown weapon
        bool isWeapon = false;
        if (!hit) return;
        if (hit.collider.gameObject.layer == 15 )
        {
            
            // if the object is the same as the last one that entered this passage, do not teleport it
            if (hit.collider.attachedRigidbody.gameObject == _lastObjectTeleported)
            {
                return;
            }

            isWeapon = true;
            ThrownWeapon weapon = hit.collider.attachedRigidbody.GetComponent<ThrownWeapon>();

            if (isWeapon)
            {
                weapon.TrailObject = Instantiate(weapon.TrailPrefab, weapon.TrailAnchor.position, Quaternion.identity, weapon.TrailAnchor);
                weapon.ChargedParticleObject = Instantiate(weapon.ChargedParticlePrefab, weapon.transform.position, weapon.transform.rotation, weapon.transform);
            }

            weapon.DetachParticle();
        }
        if (hit.collider.gameObject.layer != 3)
        {
            float distance = Mathf.Abs(hit.point.y - transform.position.y);
            Debug.Log(distance);

            hit.collider.gameObject.transform.position = new Vector2(_tpExit.position.x, _tpExit.position.y + distance) ;
            
        }


    }

    private void OnDrawGizmos()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, _maxDistance, _rayLayerMask);

        Debug.DrawRay(transform.position, Vector2.up * 10, Color.black);
    }

    
}
