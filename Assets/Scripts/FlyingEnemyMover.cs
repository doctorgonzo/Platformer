using UnityEngine;

public class FlyingEnemyMover : Enemy
{
    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float aggroRange = 6f;
    [SerializeField] private float belowPlayerOffset = 1f; // aim under the player's feet, not their center

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
            Vector2 playerPosition = player.transform.position;
            if ((playerPosition - rb.position).sqrMagnitude <= aggroRange * aggroRange)
            {
                // Approach from below so the enemy presents its top to the
                // player's feet, giving them a chance to stomp it
                Vector2 aimPoint = playerPosition + Vector2.down * belowPlayerOffset;
                desiredVelocity = (aimPoint - rb.position).normalized * speed;
            }
        }

        // Steer toward the desired velocity instead of setting it directly,
        // so the enemy has inertia and external impulses (knockback) aren't wiped
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
    }
}
