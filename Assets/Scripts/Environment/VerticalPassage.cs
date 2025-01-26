using UnityEngine;

public class VerticalPassage : MonoBehaviour
{

    [SerializeField] private Transform _connector;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Get the collision object's initial X position
        float collisionInitialX = collision.gameObject.transform.position.x;
        Debug.Log(collisionInitialX);
        Debug.Log("Collider" + collision.gameObject.name);
        // Reposition the collision object
        Vector3 newPosition = new Vector3(collisionInitialX, _connector.position.y, _connector.position.z);
        newPosition.x = collisionInitialX; // Maintain the collision object's initial X
        collision.gameObject.transform.position = newPosition;
    }
}
