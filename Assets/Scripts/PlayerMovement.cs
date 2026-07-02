using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
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

    [Header("Push")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private Transform pushCheck;
    [SerializeField] private float pushDistance = 1f;
    [SerializeField] private GameObject pushLightRight;
    [SerializeField] private GameObject pushLightLeft;
    [SerializeField] private float pushCooldown = 0.5f;
    private float lastPushTime = -Mathf.Infinity;
    private Rigidbody2D rb;
    private float moveInput;
    private bool jumpQueued;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isJumping;
    private bool jumpHeld;
    private float jumpHoldTimer;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //rb.sharedMaterial = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
        pushLightRight.SetActive(false);
        pushLightLeft.SetActive(false);
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
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);

        float targetVelocityX = moveInput * moveSpeed;
        float acceleration = isGrounded ? groundAcceleration : airAcceleration;

        float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);

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
}