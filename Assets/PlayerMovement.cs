using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class PlayerMovement : MonoBehaviour
{

    // Move player in 2D space
    public float maxSpeed = 3.4f;
    public float jumpHeight = 6.5f;
    public float climbStrength = 10.0f;
    public float ropeJump = 15.0f;
    public float gravityScale = 1.5f;
    float jumpCharge = 0.0f;
    public float jumpChargeRate = 2.0f;
    public AudioClip landClip;
    public AudioClip jumpClip;
    bool isCrouching = false;
    public Camera mainCamera;

    public bool facingRight { get; private set; } = true;
    float moveDirection = 0;
    public bool isGrounded = false;
    float defaultColliderWidth;
    float climbingColliderWidth = 0.05f;
    public bool isClimbing = false;
    float climbingXPos;
    bool prevGrounded = false;
    public Vector2 velocity;
    Vector3 cameraPos;
    Rigidbody2D r2d;
    CapsuleCollider2D mainCollider;
    SpriteRenderer spriteRenderer;
    Animator animator;
    AudioSource audioSource;

    // Use this for initialization
    void Start()
    {
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = transform.localScale.x > 0;
        defaultColliderWidth = mainCollider.size.x;

        if (mainCamera)
        {
            cameraPos = mainCamera.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //check for preventing wall climbing
        if (isClimbing && Mathf.Abs(transform.position.x - climbingXPos) > 0.01)
        {
            isClimbing = false;
            animator.SetBool("isClimbing", false);
        }

        if (isClimbing)
        {
            animator.SetBool("isClimbing", true);
            moveDirection = 0;
            // Movement controls for climbing
            if (Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.D))
            {
                moveDirection = -1;
            }
            else if (Input.GetKeyDown(KeyCode.D) && !Input.GetKeyDown(KeyCode.A))
            {
                moveDirection = 1;
            }
            else
            {
                moveDirection = 0;
            }

            //jumping away from ropes
            if (moveDirection != 0)
            {
                r2d.velocity = Vector2.zero;
                r2d.AddForce(((Vector2.right * moveDirection) + Vector2.up) * ropeJump, ForceMode2D.Impulse);
                animator.SetBool("isClimbing", false);
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))    //climbing up ropes
            {
                r2d.velocity = new Vector2(r2d.velocity.x, ropeJump);
            }
        }
        else
        {
            moveDirection = 0;
            // Movement controls
            if (Input.GetKey(KeyCode.A))
            {
                moveDirection -= 1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveDirection += 1;
            }
            else
            {
                if (isGrounded || isClimbing || r2d.velocity.magnitude < 0.01f)
                {
                    moveDirection = 0;
                }
            }

            if (isGrounded && !isClimbing)
            {
                velocity.y = 0;

                if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W))
                {
                    // Charge Jump
                    isCrouching = true;
                    moveDirection = 0;
                    jumpCharge += Time.deltaTime * jumpChargeRate;
                    if (jumpCharge > jumpHeight)
                    {
                        jumpCharge = jumpHeight;
                    }
                }
                else
                {
                    //Jump on Release
                    if (jumpCharge > 0.0f)
                    {
                        r2d.velocity = new Vector2(r2d.velocity.x, jumpCharge);
                        jumpCharge = 0.0f;
                        isCrouching = false;
                        audioSource.clip = jumpClip;
                        audioSource.Play();
                    }
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
            // Apply movement velocity
            r2d.velocity = new Vector2((moveDirection) * maxSpeed, r2d.velocity.y);
        }

        /*velocity.x = moveDirection * maxSpeed;
        velocity.y += Physics2D.gravity.y * Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime);*/

        animator.SetFloat("speed", Mathf.Abs(r2d.velocity.x));
        animator.SetBool("isCrouched", isCrouching);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isClimbing", isClimbing);

        // Camera follow
        //if (mainCamera)
        //{
        //    mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 5, cameraPos.z);
        //}
    }

    void FixedUpdate()
    {
        Bounds colliderBounds = mainCollider.bounds;
        float colliderRadius = mainCollider.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);
        // Check if player is grounded
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);
        //Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
        prevGrounded = isGrounded;
        isGrounded = false;
        if(hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit != mainCollider && !hit.CompareTag("Rope"))
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        if (!prevGrounded && isGrounded)
        {
            audioSource.clip = landClip;
            audioSource.Play();
        }


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Rope"))
        {
            isClimbing = true;
            transform.position = new Vector2(collision.transform.position.x, transform.position.y);
            climbingXPos = collision.transform.position.x;
            r2d.velocity = Vector2.zero;
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (isClimbing && !isGrounded)
    //    {
    //        isClimbing = false;
    //        animator.SetBool("isClimbing", false);
    //        r2d.velocity = Vector2.zero;
    //    }
    //}



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Rope"))
        {       
            isClimbing = false;
            animator.SetBool("isClimbing", false);
        }
    }
}
