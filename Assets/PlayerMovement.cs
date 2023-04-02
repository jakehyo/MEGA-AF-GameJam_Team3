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
    int prevDir;
    float prevPos;
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
        prevPos = transform.position.x;
        prevDir = 0;
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = transform.localScale.x > 0;
        velocity = r2d.velocity;

        if (mainCamera)
        {
            cameraPos = mainCamera.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        velocity = r2d.velocity;
        moveDirection = 0;
        
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
        else if (isGrounded)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                // Charge Jump
                velocity.x = 0;
                velocity.y = 0;
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
                if (jumpCharge > 0.0f)
                {
                    //Jump on Release
                    velocity.y = jumpCharge;
                    jumpCharge = 0.0f;
                    isCrouching = false;
                    //isGrounded = false;
                    audioSource.clip = jumpClip;
                    audioSource.Play();
                }
                else if(r2d.velocity.y < 1.0f )
                {
                    velocity.y = 0;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    moveDirection -= 1;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    moveDirection += 1;
                }

                velocity.x = moveDirection * maxSpeed;

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

        /*velocity.x = moveDirection * maxSpeed;
        velocity.y += Physics2D.gravity.y * Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime);*/

        animator.SetFloat("speed", Mathf.Abs(moveDirection));
        animator.SetBool("isCrouched", isCrouching);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isClimbing", isClimbing);
        animator.SetFloat("climbSpeed", velocity.y);
        // Apply movement velocity
        r2d.velocity = velocity;

        // Camera follow
        if (mainCamera)
        {
            mainCamera.transform.position = new Vector3(cameraPos.x, transform.position.y, cameraPos.z);
        }
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
                    ColliderDistance2D colliderDistance = hit.Distance(mainCollider);
                    if (colliderDistance.isOverlapped)
                    {
                        transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                    }
                    if (Mathf.Approximately(Vector2.Angle(colliderDistance.normal, Vector2.up), 0.0f))
                    {
                        isGrounded = true;
                    }

                    isClimbing = false;
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
