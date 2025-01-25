using UnityEngine;

public class DestructablePlatforms : Health
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Die()
    {
        base.Die();

        this.gameObject.SetActive(false);
    }
}
