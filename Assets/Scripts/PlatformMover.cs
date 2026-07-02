using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float reachThreshold = 0.05f;
    private Rigidbody2D rb;
    private bool movingRight = true;
    private Transform currentTarget;
    private Vector2 previousPosition;
    private Vector2 frameDelta;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        currentTarget = pointB;
        previousPosition = rb.position;
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, currentTarget.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        frameDelta = newPosition - previousPosition;
        previousPosition = newPosition;

        if (Vector2.Distance(rb.position, currentTarget.position) < reachThreshold)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.rigidbody;
        if (otherRb == null) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.y) > 0.5f && contact.point.y > rb.position.y)
            {
                otherRb.position += frameDelta;
                break;
            }
        }
    }
}
