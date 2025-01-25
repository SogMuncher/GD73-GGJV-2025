using UnityEngine;

public class Passages : MonoBehaviour
{
    [SerializeField] private Transform _connector;
    [SerializeField] private LayerMask _ignoredLayers;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3 )
        {
            Vector3 position = collision.transform.position;
            position.x = this._connector.position.x;
            position.y = this._connector.position.y;
            collision.transform.position = position;

        }
    }
}
