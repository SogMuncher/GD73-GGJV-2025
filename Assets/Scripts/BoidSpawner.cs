using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    public GameObject boidPrefab;
    public int numBoids = 50;
    public Vector2 spawnArea = new Vector2(10f, 10f);

    void Start()
    {
        for (int i = 0; i < numBoids; i++)
        {
            Vector2 position = new Vector2(
                Random.Range(-spawnArea.x, spawnArea.x),
                Random.Range(-spawnArea.y, spawnArea.y)
            );
            Instantiate(boidPrefab, position, Quaternion.identity);
        }
    }
}
