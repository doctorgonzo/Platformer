using UnityEngine;

public class GroundEnemyMover : Enemy
{
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 3f;
    [Header("Waypoints")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float reachThreshold = 0.25f;
    private Transform currentTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        currentTarget = pointB;
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, currentTarget.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(newPosition, currentTarget.position) < reachThreshold)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }
}
