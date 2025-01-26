using UnityEngine;

public class Passages : MonoBehaviour
{
    [SerializeField] private Transform _connector;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if the object is a thrown weapon
        bool isWeapon = false;
        ThrownWeapon weapon = null;
        if (collision.gameObject.layer == 15)
        {
            isWeapon = true;
            weapon = collision.attachedRigidbody.GetComponent<ThrownWeapon>();

            weapon.DetachParticle();
        }

        if (collision.gameObject.layer != 3)
        {

            // Get the collision object's initial X position
            float collisionInitialY = collision.gameObject.transform.position.y;
            Debug.Log(collisionInitialY);
            Debug.Log("Collider" + collision.gameObject.name);
            // Reposition the collision object
            Vector3 newPosition = new Vector3(_connector.position.x, collisionInitialY, _connector.position.z);
            newPosition.y = collisionInitialY; // Maintain the collision object's initial X
            collision.gameObject.transform.position = newPosition;

        }

        if (isWeapon)
        {
            weapon._trailObject = Instantiate(weapon._trailPrefab, weapon._trailAnchor.position, Quaternion.identity, weapon._trailAnchor);
        }
    }
}
