using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour
{
    // Adjustable parameters
    public float speed = 2f;
    public float neighborRadius = 2f;
    public float avoidanceRadius = 1f;
    public float obstacleAvoidanceRadius = 1.5f;
    public float maxForce = 0.5f;

    [Range(0f, 5f)] public float alignmentWeight = 1f;
    [Range(0f, 5f)] public float cohesionWeight = 1f;
    [Range(0f, 5f)] public float separationWeight = 1f;
    [Range(0f, 5f)] public float obstacleAvoidanceWeight = 1f;

    public LayerMask obstacleLayers;
    public bool avoidObstacles = true; // Toggle obstacle avoidance

    private Vector2 velocity;
    private List<Boid> boids;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        velocity = Random.insideUnitCircle.normalized * speed;
        boids = new List<Boid>(FindObjectsByType<Boid>(FindObjectsSortMode.None));
        boids.Remove(this);
    }

    void FixedUpdate()
    {
        Vector2 acceleration = Vector2.zero;

        Vector2 alignment = ComputeAlignment() * alignmentWeight;
        Vector2 cohesion = ComputeCohesion() * cohesionWeight;
        Vector2 separation = ComputeSeparation() * separationWeight;
        Vector2 obstacleAvoidance = Vector2.zero;

        if (avoidObstacles)
        {
            obstacleAvoidance = ComputeObstacleAvoidance() * obstacleAvoidanceWeight;
        }

        acceleration += alignment + cohesion + separation + obstacleAvoidance;

        rb.AddForce(acceleration);

        // Limit speed
        if (rb.linearVelocity.magnitude > speed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }

        transform.up = rb.linearVelocity.normalized;
    }

    Vector2 ComputeAlignment()
    {
        Vector2 steering = Vector2.zero;
        int count = 0;

        foreach (Boid other in boids)
        {
            if (IsNeighbor(other))
            {
                steering += other.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            steering /= count;
            steering = steering.normalized * speed;
            steering -= rb.linearVelocity;
            steering = Vector2.ClampMagnitude(steering, maxForce);
        }

        return steering;
    }

    Vector2 ComputeCohesion()
    {
        Vector2 steering = Vector2.zero;
        int count = 0;

        foreach (Boid other in boids)
        {
            if (IsNeighbor(other))
            {
                steering += (Vector2)other.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            steering /= count;
            steering -= (Vector2)transform.position;
            steering = steering.normalized * speed;
            steering -= rb.linearVelocity;
            steering = Vector2.ClampMagnitude(steering, maxForce);
        }

        return steering;
    }

    Vector2 ComputeSeparation()
    {
        Vector2 steering = Vector2.zero;
        int count = 0;

        foreach (Boid other in boids)
        {
            float distance = Vector2.Distance(transform.position, other.transform.position);
            if (distance < avoidanceRadius && other != this)
            {
                Vector2 difference = (Vector2)transform.position - (Vector2)other.transform.position;
                steering += difference.normalized / distance;
                count++;
            }
        }

        if (count > 0)
        {
            steering /= count;
            steering = steering.normalized * speed;
            steering -= rb.linearVelocity;
            steering = Vector2.ClampMagnitude(steering, maxForce);
        }

        return steering;
    }

    Vector2 ComputeObstacleAvoidance()
    {
        Vector2 steering = Vector2.zero;

        // Define the number of rays and the angle between them
        int rayCount = 5;
        float rayAngleRange = 90f; // Total angle range to cover
        float raySpacing = rayAngleRange / (rayCount - 1);
        float baseAngle = -rayAngleRange / 2f;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = baseAngle + (raySpacing * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 direction = rotation * rb.linearVelocity.normalized;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleAvoidanceRadius, obstacleLayers);

            if (hit.collider != null)
            {
                Vector2 hitNormal = hit.normal;
                steering += hitNormal;
                // Optionally, break after the first hit for efficiency
            }

            // Visualize the rays (for debugging)
            Debug.DrawRay(transform.position, direction * obstacleAvoidanceRadius, Color.red);
        }

        if (steering != Vector2.zero)
        {
            steering = steering.normalized * speed;
            steering -= rb.linearVelocity;
            steering = Vector2.ClampMagnitude(steering, maxForce);
        }

        return steering;
    }

    bool IsNeighbor(Boid other)
    {
        return Vector2.Distance(transform.position, other.transform.position) < neighborRadius;
    }
}
