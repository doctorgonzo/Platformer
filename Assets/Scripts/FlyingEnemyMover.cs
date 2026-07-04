using UnityEngine;

public class FlyingEnemyMover : Enemy
{
    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float aggroRange = 6f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 desiredVelocity = Vector2.zero;

        Player player = Player.Instance;
        if (player != null && !player.IsDead)
        {
            Vector2 toPlayer = (Vector2)player.transform.position - rb.position;
            if (toPlayer.sqrMagnitude <= aggroRange * aggroRange)
            {
                desiredVelocity = toPlayer.normalized * speed;
            }
        }

        // Steer toward the desired velocity instead of setting it directly,
        // so the enemy has inertia and external impulses (knockback) aren't wiped
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
    }
}
