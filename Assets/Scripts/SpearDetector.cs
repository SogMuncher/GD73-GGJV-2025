using UnityEngine;

public class SpearDetector : MonoBehaviour
{
    [SerializeField] private float _collisionRadius;
    private CircleCollider2D _collider;

    private void Start()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.radius = _collisionRadius;
    }
}
