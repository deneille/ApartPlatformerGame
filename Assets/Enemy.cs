using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;
    public int damage = 1;
    public bool isDistracted = false;
    public float distractionDuration = 5f; // How long the enemy stays distracted

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;
    private Vector2 distractionPosition;
    private bool hasDistractionTarget = false;
    private float distractionTimer = 0f;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
    }
    
    void Update(){
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
            if (distractionTimer >= distractionDuration) {
                isDistracted = false;
                hasDistractionTarget = false;
                distractionTimer = 0f;
                Debug.Log("Enemy no longer distracted");
            }
        }

        // If distracted, look for distraction items
        if (isDistracted) {
            if (!hasDistractionTarget) {
                // Find the nearest distraction item
                Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 10f);
                float closestDistance = float.MaxValue;
                
                foreach (Collider2D collider in nearbyColliders) {
                    // Check if this is a distraction item
                    MonoBehaviour[] components = collider.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour component in components) {
                        if (component is IDistractionItem) {
                            IDistractionItem distractionItem = component as IDistractionItem;
                            float distance = Vector2.Distance(transform.position, distractionItem.GetPosition());
                            if (distance < closestDistance) {
                                closestDistance = distance;
                                distractionPosition = distractionItem.GetPosition();
                                hasDistractionTarget = true;
                                Debug.Log("Found distraction target at: " + distractionPosition);
                            }
                        }
                    }
                }
            }

            if (hasDistractionTarget) {
                // Move towards distraction
                float distractionDirection = Mathf.Sign(distractionPosition.x - transform.position.x);
                if (isGrounded) {
                    rb.velocity = new Vector2(distractionDirection * chaseSpeed, rb.velocity.y);
                }

                // If we're close enough to the distraction, stop being distracted
                if (Vector2.Distance(transform.position, distractionPosition) < 0.5f) {
                    Debug.Log("Reached distraction target");
                    // Don't reset distraction here, let the timer handle it
                }
                return; // Skip normal movement logic
            }
        }

        // Normal movement logic (when not distracted)
        if (!isDistracted) {
            // Player Direction
            float direction = Mathf.Sign(player.position.x - transform.position.x);

            // Check for edges
            Vector2 edgeCheckStart = (Vector2)transform.position + new Vector2(direction * 0.5f, 0);
            RaycastHit2D edgeCheck = Physics2D.Raycast(edgeCheckStart, Vector2.down, 2f, groundLayer);
            Debug.DrawRay(edgeCheckStart, Vector2.down * 2f, Color.magenta); // Visualize edge detection

            if (isGrounded) {
                // Only move if there's ground ahead
                if (edgeCheck.collider != null) {
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

                // Modified jump conditions
                if (groundInFront.collider != null && !gapAhead.collider) {
                    shouldJump = true;
                    Debug.Log("Should jump due to obstacle ahead");
                }
                else if (isPlayerAbove && platformAbove.collider && Vector2.Distance(transform.position, player.position) < 5f) {
                    shouldJump = true;
                    Debug.Log("Should jump due to player above and in range");
                }
            } else {
                // Add some air control
                rb.velocity = new Vector2(direction * chaseSpeed * 0.5f, rb.velocity.y);
            }
        }
    }

    void FixedUpdate()
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
    
}
