using UnityEngine;
using UnityEngine.Events;

[RequireComponent (typeof(Collider2D))]
public class HitBox : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<Collider2D> OnTriggerEnterEvent;

    [HideInInspector]
    public UnityEvent<Collision2D> OnCollisionEnterEvent;

    protected Collider2D _collider;

    private void OnEnable()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionEnterEvent.Invoke(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnTriggerEnterEvent.Invoke(collision);
    }

    public void ChangeIncludeLayerMask(LayerMask layerMask)
    {
        _collider.includeLayers = layerMask;
    }

    public void ChangeExcludeLayerMask(LayerMask layerMask)
    {
        _collider.excludeLayers = layerMask;
    }

    public void ColliderIsTrigger(bool isTrigger)
    {
        _collider.isTrigger = isTrigger;
    }
}
