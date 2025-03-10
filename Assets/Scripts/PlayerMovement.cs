using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    [Header("Movement")]
    public float moveSpeed = 5f;
    [Header("Jumping")]
    public float jumpForce = 10f;
    public int maxJumps = 2;
    int jumps;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.5f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallGravityMultiplier = 0.8f;

    public Animator animator;
    bool isFacingRight = true;
    float horizontalMovement;

    private bool wasGrounded = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
        GroundCheck();
        // UpdateGravity();
        animator.SetFloat("velocityY", rb.velocity.y);
        animator.SetFloat("velocityX", rb.velocity.x);
        animator.SetBool("isGrounded", isGrounded());
        animator.SetBool("isHorizontalMoving", horizontalMovement != 0);

        flip();
        

    }

    public void UpdateGravity()
    {
        if (rb.velocity.y < 0 && rb.gravityScale != baseGravity * fallGravityMultiplier)
        {
            rb.gravityScale = baseGravity * fallGravityMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else if (rb.velocity.y >= 0 && rb.gravityScale != baseGravity)
        {
            rb.gravityScale = baseGravity;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumps > 0)
        {
            if (context.performed)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumps--;
                animator.SetTrigger("jump");
            }
            else if (context.canceled && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.1f);
                
            }
        }
        
    }

    private void GroundCheck()
    {
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
        if(grounded && !wasGrounded){
            jumps = maxJumps;
        }
        wasGrounded = grounded;
    }
    public bool isGrounded(){
        return jumps == maxJumps;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
    private void flip(){
        if(isFacingRight && horizontalMovement < 0f || !isFacingRight && horizontalMovement > 0f){
            isFacingRight = !isFacingRight;
            transform.localScale = new Vector3(
            isFacingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
            
        }
    }
}
