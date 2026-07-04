using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float minSpeed = 8f;
    [SerializeField] private float groundAcceleration = 60f; // ground: near-instant response
    [SerializeField] private float airAcceleration = 12f;    // air: slow, limited steering

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;       // initial tap impulse
    [SerializeField] private float jumpHoldForce = 20f;   // extra force while held
    [SerializeField] private float maxJumpHoldTime = 0.25f;


    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Stomp")]
    [SerializeField] private float stompBounceVelocity = 8f;

    [Header("Push")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private Transform pushCheck;
    [SerializeField] private float pushDistance = 1f;
    [SerializeField] private GameObject pushLightRight;
    [SerializeField] private GameObject pushLightLeft;
    [SerializeField] private float pushCooldown = 0.5f;
    private float lastPushTime = -Mathf.Infinity;
    public Rigidbody2D rb;
    private float moveInput;
    private bool jumpQueued;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isJumping;
    private bool jumpHeld;
    private float jumpHoldTimer;
    [Header("Spawn Point")]
    [SerializeField] public GameObject spawnPoint;
    public static PlayerMovement Instance { get; private set; }
    [Header("Knockback and Invulnerability")]
    [SerializeField] private float knockbackForce = 3f; // Force applied when taking damage
    [SerializeField] private float knockbackUpwardVelocity = 5f; // pop up as well as away, so knockback breaks contact
    [SerializeField] private float knockbackDuration = 0.2f; // movement control suspended while knocked back
    [SerializeField] private float invulnerabilityDuration = 1f; // i-frames after a hit
    private float knockbackTimer;
    private float invulnerabilityTimer;
    private float velocityYBeforeSolve; // fall speed going into the physics solve, see HandleEnemyContact

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //rb.sharedMaterial = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
        pushLightRight.SetActive(false);
        pushLightLeft.SetActive(false);
        gameObject.transform.position = spawnPoint.transform.position;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GetComponent<SpriteRenderer>().color = Color.green;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput > 0)
        {
            facingRight = true;
        }
        if (moveInput < 0)
        {
            facingRight = false;
        }
        if (Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
            jumpHeld = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
        }
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= lastPushTime + pushCooldown)
        {
            DoPush();
            lastPushTime = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gameObject.transform.position = spawnPoint.transform.position;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (/* moveInput != 0 && */ isGrounded)
            {
                moveSpeed = maxSpeed;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed = minSpeed;
        }
    }

    private void FixedUpdate()
    {
        Collider2D groundHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);
        isGrounded = groundHit != null;
        PlatformMover platform = isGrounded ? groundHit.GetComponentInParent<PlatformMover>() : null;
        Vector2 platformVelocity = platform != null ? platform.Velocity : Vector2.zero;
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.fixedDeltaTime;
        }
        // While knocked back, leave velocity alone: both the input steering and the
        // platform ride below would otherwise erase the knockback within a few steps.
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
        }
        else
        {
            float targetVelocityX = moveInput * moveSpeed + platformVelocity.x;
            float acceleration = isGrounded ? groundAcceleration : airAcceleration;
            float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
            // Ride the platform: match its vertical velocity so gravity/depenetration can't cause bouncing
            if (platform != null && !isJumping)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, platformVelocity.y);
            }
        }
        if (jumpQueued)
        {
            if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
            jumpHoldTimer = 0f;
        }
            jumpQueued = false;
        }
        if (isJumping)
    {
        if (jumpHeld && jumpHoldTimer < maxJumpHoldTime)
        {
            rb.AddForce(Vector2.up * jumpHoldForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
            jumpHoldTimer += Time.fixedDeltaTime;
        }
        else
        {
            isJumping = false;
        }

        if (rb.linearVelocity.y <= 0f)
        {
            isJumping = false; // stop adding force once falling
        }
    }
        // FixedUpdate runs before the physics solve, so this is our true velocity
        // at impact time — unlike rb.linearVelocity read inside collision callbacks.
        velocityYBeforeSolve = rb.linearVelocity.y;
    }

    private void DoPush()
    {
        if (facingRight)
        {
            pushLightRight.SetActive(true);
        }
        else
        {
            pushLightLeft.SetActive(true);
        }
        StartCoroutine(pushLightTimer());
        RaycastHit2D hit = facingRight ? Physics2D.Raycast(pushCheck.position, Vector2.right * transform.localScale.x, pushDistance) : Physics2D.Raycast(pushLightLeft.transform.position, Vector2.left * transform.localScale.x, pushDistance);
        if (hit.collider != null)
        {
            Rigidbody2D hitRb = hit.collider.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                if (facingRight)
                {
                    hitRb.AddForce(Vector2.right * transform.localScale.x * pushForce, ForceMode2D.Impulse);
                }
                else
                {
                    hitRb.AddForce(Vector2.left * transform.localScale.x * pushForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    private IEnumerator pushLightTimer()
    {
        yield return new WaitForSeconds(.2f);
        pushLightRight.SetActive(false);
        pushLightLeft.SetActive(false);
    }

    // Stay as well as Enter: if knockback doesn't fully separate us from the enemy
    // (or it walks back into us), the contact never re-Enters, so damage would
    // otherwise only ever apply once per touch. The i-frame timer sets the pace.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEnemyContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleEnemyContact(collision);
    }

    private void HandleEnemyContact(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            return;
        }
        // A stomp needs both: a top contact AND downward motion at impact.
        // The contact normal points from the enemy toward us, so y > 0.5 means
        // we hit its top (allows up to ~60 degrees off vertical) — but corner
        // grazes while jumping up past the enemy also produce an upward normal,
        // so require that we were actually falling. rb.linearVelocity is already
        // zeroed by the solver in this callback, hence the cached pre-solve value.
        bool wasFalling = velocityYBeforeSolve < -0.1f;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (wasFalling && collision.GetContact(i).normal.y > 0.5f)
            {
                enemy.OnStomped();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceVelocity);
                return;
            }
        }
        if (invulnerabilityTimer > 0f)
        {
            return;
        }
        // Take damage and get knocked away from the enemy. The contact normal's
        // x sign is the push direction (hit from the right -> normal.x < 0 ->
        // push left); for near-vertical normals (corner grazes) fall back to
        // which side of the enemy we're on.
        Debug.Log("Hit by: " + collision.gameObject.name);
        float normalX = collision.GetContact(0).normal.x;
        float knockbackDirection = Mathf.Abs(normalX) > 0.01f
            ? Mathf.Sign(normalX)
            : Mathf.Sign(transform.position.x - collision.transform.position.x);
        rb.linearVelocity = new Vector2(knockbackDirection * knockbackForce, knockbackUpwardVelocity);
        knockbackTimer = knockbackDuration;
        invulnerabilityTimer = invulnerabilityDuration;
        Player.Instance.TakeDamage(1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DeathZone"))
        {
            Player.Instance.TakeDamage(100);
        }
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            spawnPoint.transform.position = collision.gameObject.transform.GetChild(0).transform.position;
            Destroy(collision.gameObject);
        }
    }
}