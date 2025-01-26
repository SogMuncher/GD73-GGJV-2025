using UnityEngine;

public class Passages : MonoBehaviour
{
    [SerializeField] private Transform _connector;

    private void OnTriggerEnter2D(Collider2D collision)
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
}
