using UnityEngine;

public class Passages : MonoBehaviour
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
            //_connector.GetComponent<Passages>()._lastObjectTeleported = _lastObjectTeleported;

            // Get the collision object's initial X position
            float collisionInitialY = collision.gameObject.transform.position.y;
            Debug.Log(collisionInitialY);
            Debug.Log("Collider" + collision.gameObject.name);
            // Reposition the collision object
            Vector3 newPosition = new Vector3(_connector.position.x, collisionInitialY, _connector.position.z);
            newPosition.y = collisionInitialY; // Maintain the collision object's initial Y
            collision.attachedRigidbody.gameObject.transform.position = newPosition;
        }

        if (isWeapon)
        {
            weapon.TrailObject = Instantiate(weapon.TrailPrefab, weapon.TrailAnchor.position, Quaternion.identity, weapon.TrailAnchor);
        }
    }
}
