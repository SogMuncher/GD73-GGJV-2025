using System.Collections;
using UnityEngine;

public class DestructablePlatforms : Health
{

    [SerializeField] protected GameObject _clashParticlePrefab;


    protected override void Die()
    {
        Instantiate(_clashParticlePrefab, transform.position, Quaternion.identity);
        OnDeathEvent.Invoke();
        Destroy(gameObject);
    }

}
