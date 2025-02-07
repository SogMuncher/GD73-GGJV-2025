using UnityEngine;

public class VerticalPassage : MonoBehaviour
{

    [SerializeField] private Transform _connector;

    [HideInInspector]
    public GameObject _lastObjectTeleported;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if the object is a thrown weapon
        bool isWeapon = false;
        ThrownWeapon weapon = null;
        if (collision.gameObject.layer == 15)
        {
            // if the object is the same as the last one that entered this passage, do not teleport it
            if (collision.attachedRigidbody.gameObject == _lastObjectTeleported)
            {
                return;
            }

            isWeapon = true;
            weapon = collision.attachedRigidbody.GetComponent<ThrownWeapon>();

            weapon.DetachParticle();
        }

        if (collision.gameObject.layer != 3)
        {
            _lastObjectTeleported = collision.attachedRigidbody.gameObject;
            //_connector.GetComponent<VerticalPassage>()._lastObjectTeleported = _lastObjectTeleported;

            // Get the collision object's initial X position
            float collisionInitialX = collision.gameObject.transform.position.x;
            Debug.Log(collisionInitialX);
            Debug.Log("Collider" + collision.gameObject.name);
            // Reposition the collision object
            Vector3 newPosition = new Vector3(collisionInitialX, _connector.position.y, _connector.position.z);
            newPosition.x = collisionInitialX; // Maintain the collision object's initial X
            collision.attachedRigidbody.gameObject.transform.position = newPosition;
        }

        if (isWeapon)
        {
            weapon.TrailObject = Instantiate(weapon.TrailPrefab, weapon.TrailAnchor.position, Quaternion.identity, weapon.TrailAnchor);
        }
    }
}
