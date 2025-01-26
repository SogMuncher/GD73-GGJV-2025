using UnityEngine;

public class SpriteRotator : MonoBehaviour
{

    [SerializeField] private float _speed = 3f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * _speed * Time.deltaTime);
    }
}
