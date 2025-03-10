using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ApartPlatformer;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;
    public float detectionRange = 5f;

    public int damage = 1;
    public bool isDistracted = false;
    bool  isFacingRight = true;
    public float distractionDuration = 3f; // How long the enemy stays distracted

    protected Rigidbody2D rb;
    protected bool isGrounded;
    protected bool shouldJump;
    protected Vector2 distractionPosition;
    protected bool hasDistractionTarget = false;
    protected float distractionTimer = 0f;

    protected virtual void Start(){
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
    }
    
    protected virtual void Update(){
        
        if (Mathf.Abs(rb.velocity.x) > 0.1f) {
            flip();
        }
        // Check if we're on the ground
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 2f, Color.red); // Visualize ground detection ray

        if(player == null) {
            Debug.LogError("Player reference is missing!");
            return;
        }

        // Handle distraction timer
        if (isDistracted) {
            distractionTimer += Time.deltaTime;
            
            // Visual indicator that enemy is distracted
            if (GetComponent<SpriteRenderer>() != null)
            {
                // Flash the sprite to indicate distraction
                GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0.5f, 0.8f + Mathf.PingPong(Time.time * 2, 0.2f));
            }
            
            if (distractionTimer >= distractionDuration) {
                isDistracted = false;
                hasDistractionTarget = false;
                distractionTimer = 0f;
                
                // Reset sprite color
                if (GetComponent<SpriteRenderer>() != null)
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                }
                
                Debug.Log(gameObject.name + " is no longer distracted");
            }
        }

        // If distracted, move towards the distraction target
        if (isDistracted && hasDistractionTarget) {
            // Calculate direction to distraction
            float distractionDirection = Mathf.Sign(distractionPosition.x - transform.position.x);
            
            // Check if moving beyond a boundary - only if very close to boundary
            bool beyondBoundary = false;
            if (Mathf.Abs(transform.position.x - distractionPosition.x) < 1.0f)
            {
                //beyondBoundary = ProximityTrigger2D.IsBeyondBoundary(transform.position, new Vector2(distractionDirection, 0));
            }
            
            // If the distraction is beyond a boundary, stop being distracted
            if (beyondBoundary)
            {
                // Check if the distraction itself is beyond the boundary
                //bool distractionBeyondBoundary = ProximityTrigger2D.IsBeyondBoundary(distractionPosition, new Vector2(distractionDirection, 0));
                bool distractionBeyondBoundary = false;
                if (distractionBeyondBoundary)
                {
                    // Stop being distracted by items beyond boundaries
                    isDistracted = false;
                    hasDistractionTarget = false;
                    distractionTimer = 0f;
                    
                    // Reset sprite color
                    if (GetComponent<SpriteRenderer>() != null)
                    {
                        GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    
                    Debug.Log(gameObject.name + " stopped being distracted by item beyond boundary");
                    return;
                }
                
                // Stop at the boundary
                rb.velocity = new Vector2(0, rb.velocity.y);
                Debug.Log("Distracted enemy stopped at boundary");
            }
            else
            {
                // Move towards distraction if grounded and not beyond boundary
                if (isGrounded) {
                    rb.velocity = new Vector2(distractionDirection * chaseSpeed, rb.velocity.y);
                    Debug.Log(gameObject.name + " moving towards distraction, velocity: " + rb.velocity);
                }
            }

            // If we're close enough to the distraction, stop moving
            float distanceToDistraction = Vector2.Distance(transform.position, distractionPosition);
            if (distanceToDistraction < 0.5f) {
                rb.velocity = new Vector2(0, rb.velocity.y);
                Debug.Log(gameObject.name + " reached distraction target");
            }
            
            // Skip normal movement logic
            return;
        }

        // Normal movement logic (when not distracted)
        if (Vector2.Distance(transform.position, player.position) < detectionRange)
        {
            if (!isDistracted) {
            // Player Direction
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            
            // Check if moving beyond a boundary - only if very close to boundary
            bool beyondBoundary = false;
            if (Mathf.Abs(transform.position.x - player.position.x) < 1.0f)
            {
                //beyondBoundary = ProximityTrigger2D.IsBeyondBoundary(transform.position, new Vector2(direction, 0));
            }
            
            // If beyond boundary, stop or turn around
            if (beyondBoundary)
            {
                // Stop at the boundary
                rb.velocity = new Vector2(0, rb.velocity.y);
                Debug.Log("Enemy stopped at boundary");
                
                // Optional: Turn around
                direction = -direction;
            }

            // Check for edges
            Vector2 edgeCheckStart = (Vector2)transform.position + new Vector2(direction * 0.5f, 0);
            RaycastHit2D edgeCheck = Physics2D.Raycast(edgeCheckStart, Vector2.down, 2f, groundLayer);
            Debug.DrawRay(edgeCheckStart, Vector2.down * 2f, Color.magenta); // Visualize edge detection

            if (isGrounded) {
                // Only move if there's ground ahead and not beyond a boundary
                if (edgeCheck.collider != null && !beyondBoundary) {
                    // Chase player
                    rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
                } else {
                    // Stop at edge if no ground ahead
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }

                // Jump logic
                RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);
                Debug.DrawRay(transform.position, new Vector2(direction, 0) * 2f, Color.green); // Visualize forward detection ray

                RaycastHit2D gapAhead = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction, 0), Vector2.down, 2f, groundLayer);
                Debug.DrawRay((Vector2)transform.position + new Vector2(direction, 0), Vector2.down * 2f, Color.yellow); // Visualize gap detection ray

                // Player above detection
                bool isPlayerAbove = player.CompareTag("Player") && Physics2D.Raycast(transform.position, Vector2.up, 3f, groundLayer);
                Debug.DrawRay(transform.position, Vector2.up * 3f, Color.blue); // Visualize player above detection ray
                
                RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, groundLayer);

                // Modified jump conditions - don't jump if it would cross a boundary
                if (!beyondBoundary && groundInFront.collider != null && gapAhead.collider) {
                    shouldJump = true;
                    Debug.Log(gameObject.name + " is jumping over an obstacle.");
                }

                else if (!beyondBoundary && isPlayerAbove && platformAbove.collider && Vector2.Distance(transform.position, player.position) < 5f) {
                    shouldJump = true;
                    Debug.Log("Should jump due to player above and in range");
                }
            } else {
                // Add some air control, but respect boundaries
                if (!beyondBoundary) {
                    rb.velocity = new Vector2(direction * chaseSpeed * 0.5f, rb.velocity.y);
                } else {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }
        }
        }
        else{
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
    }

    protected virtual void FixedUpdate()
    {
        // Enemy Jumps
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 playerDirection = (player.position - transform.position).normalized;
            Vector2 jumpVelocity = new Vector2(playerDirection.x * jumpForce, jumpForce);

            rb.AddForce(jumpVelocity, ForceMode2D.Impulse);
            Debug.Log("Jumping with velocity: " + jumpVelocity);
        }
    }

    Vector2 FindDistractionItemPosition()
    {
        // Add your logic to find the distraction item. For simplicity, returning a point close to the player.
        return player.position; // or a distraction item's position if you want
    }

    public bool IsDistracted
    {
        get { return isDistracted; }
        set { isDistracted = value; }
    }

    public void SetDistractionTarget(Vector2 targetPosition)
    {
        distractionPosition = targetPosition;
        hasDistractionTarget = true;
        isDistracted = true;
        distractionTimer = 0f;
        Debug.Log(gameObject.name + " distraction target set to: " + targetPosition);
    }
    private void flip(){
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        if(isFacingRight && direction < 0f || !isFacingRight && direction > 0f){
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
            
        }
    }
}
