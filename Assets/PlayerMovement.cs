using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class PlayerMovement : MonoBehaviour
{

    // Move player in 2D space
    public float maxSpeed = 3.4f;
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;
    public float jumpCharge = 0.0f;
    public float jumpChargeRate = 2.0f;
    public bool isCrouching = false;
    public Camera mainCamera;

    public bool facingRight { get; private set; } = true;
    float moveDirection = 0;
    bool isGrounded = false;
    Vector3 cameraPos;
    Rigidbody2D r2d;
    CapsuleCollider2D mainCollider;
    SpriteRenderer spriteRenderer;
    Transform t;

    // Use this for initialization
    void Start()
    {
        t = transform;
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;

        if (mainCamera)
        {
            cameraPos = mainCamera.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) {
            // Charge Jump
            
            if (isGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
                {
                    isCrouching = true;
                }
                moveDirection = 0;
                jumpCharge += Time.deltaTime * jumpChargeRate;
                if (jumpCharge > jumpHeight)
                {
                    jumpCharge = jumpHeight;
                }
            }
        }
        else
        {
            
            // Movement controls
            if (isGrounded)
            {
                moveDirection = 0;
                if (Input.GetKey(KeyCode.A)){
                    moveDirection -= 1;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    moveDirection += 1;
                }

            }

            if (jumpCharge > 0.0f && isGrounded)
            {
                r2d.velocity = (new Vector2((moveDirection) * maxSpeed, jumpCharge).normalized) * jumpCharge;
                jumpCharge = 0.0f;
                isCrouching = false;
            }

            // Change facing direction
            if (moveDirection != 0)
            {
                if (moveDirection > 0 && !facingRight)
                {
                    facingRight = true;
                    spriteRenderer.flipX = false;
                }
                if (moveDirection < 0 && facingRight)
                {
                    facingRight = false;
                    spriteRenderer.flipX = true;
                }
            }
        }

        if (isCrouching)
        {
            spriteRenderer.color = Color.red;
        }
        else
        {
            spriteRenderer.color = Color.green;
        }

        // Camera follow
/*        if (mainCamera)
        {
            mainCamera.transform.position = new Vector3(t.position.x, cameraPos.y, cameraPos.z);
        }*/
    }

    void FixedUpdate()
    {
        Bounds colliderBounds = mainCollider.bounds;
        float colliderRadius = mainCollider.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);
        // Check if player is grounded
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);
        //Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
        isGrounded = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != mainCollider)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Apply movement velocity
        r2d.velocity = new Vector2((moveDirection) * maxSpeed, r2d.velocity.y);

        // Simple debug
        Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(0, colliderRadius, 0), isGrounded ? Color.green : Color.red);
        Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(colliderRadius, 0, 0), isGrounded ? Color.green : Color.red);
    }
}
