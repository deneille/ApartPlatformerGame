using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ApartPlatformer;

public class Boss : Enemy
{
    public bool canPassBoundary = true;  // Initially can pass through boundaries
    public float attackRange = 2f;       // Range at which the boss can attack
    public int attackDamage = 2;         // Damage dealt by boss attacks
    public float attackCooldown = 1.5f;  // Time between attacks
    public float initialMoveDelay = 1f;  // Delay before boss starts moving after spawn
    public bool canMove = false; // 🔹 Boss starts stationary
    public DoorController linkedDoor;  // Reference to the door
    public float candyCaneCheckDistance = 0.5f; 
    public GameObject wholeandyCane;
    
    private float lastAttackTime = 0f;   // Track when the last attack occurred
    private bool isBlocked = false;      // Whether the boss is blocked by a boundary
    private bool hasStartedMoving = false; // Whether the boss has started moving
    
    // Override the Update method to handle special boss behavior
    protected new void Update()
    {
        // Check if we're on the ground
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);

        if (!canMove) return;  // 🚫 Prevent movement until allowed

        if (IsCandyCaneInFront(wholeandyCane))
        {
            StopBossMovementAndAttack();
            return;  // Skip the normal update behavior to keep the boss idle
        }
        
        // If we haven't started moving yet, start the initial movement
        // if (!hasStartedMoving)
        // {
        //     StartCoroutine(InitialMovement());
        //     hasStartedMoving = true;
        //     return;
        // }
        
        // If player is null, try to find it
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return; // Can't do anything without a player
            }
        }
        
        // Calculate direction to player
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        
        // Check if we're beyond a boundary and should be blocked
        if (!canPassBoundary)
        {
            // bool beyondBoundary = ProximityTrigger2D.IsBeyondBoundary(transform.position, new Vector2(direction, 0));
            bool beyondBoundary = false;
            if (beyondBoundary)
            {
                isBlocked = true;
                // Try to attack if player is in range
                TryAttackPlayer();
                return; // Don't move if blocked by boundary
            }
            else
            {
                isBlocked = false;
            }
        }
        
        // Handle distraction behavior
        if (isDistracted && hasDistractionTarget)
        {
            distractionTimer += Time.deltaTime;
            
            // Check if distraction has expired
            if (distractionTimer >= distractionDuration)
            {
                isDistracted = false;
                hasDistractionTarget = false;
                Debug.Log(gameObject.name + " is no longer distracted");
                return;
            }
            
            // Calculate direction to distraction
            float distractionDirection = Mathf.Sign(distractionPosition.x - transform.position.x);
            
            // Check if we're beyond a boundary when moving toward distraction
            if (!canPassBoundary)
            {
                // bool beyondBoundary = ProximityTrigger2D.IsBeyondBoundary(transform.position, new Vector2(distractionDirection, 0));
                bool beyondBoundary = false;
                if (beyondBoundary)
                {
                    // Stop being distracted if the distraction is beyond the boundary
                    isDistracted = false;
                    hasDistractionTarget = false;
                    Debug.Log(gameObject.name + " stopped being distracted because distraction is beyond boundary");
                    return;
                }
            }
            
            // Move towards distraction if grounded
            if (isGrounded)
            {
                // Move towards the distraction
                rb.velocity = new Vector2(distractionDirection * chaseSpeed, rb.velocity.y);
                
                // Check for edge
                Vector2 edgeCheckPosition = new Vector2(transform.position.x + distractionDirection * 0.5f, transform.position.y);
                bool isGroundAhead = Physics2D.Raycast(edgeCheckPosition, Vector2.down, 2f, groundLayer);
                
                // Jump if there's no ground ahead and we're not beyond a boundary
                // if (!isGroundAhead && !ProximityTrigger2D.IsBeyondBoundary(edgeCheckPosition, new Vector2(distractionDirection, 0)))
                // {
                //     shouldJump = true;
                // }
            }
            
            // Handle jumping
            if (shouldJump && isGrounded)
            {
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                shouldJump = false;
            }
            
            return; // Skip normal movement when distracted
        }
        
        // Normal movement behavior
        if (isGrounded)
        {
            // Move towards the player
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
            
            // Check for edge
            Vector2 edgeCheckPosition = new Vector2(transform.position.x + direction * 0.5f, transform.position.y);
            bool isGroundAhead = Physics2D.Raycast(edgeCheckPosition, Vector2.down, 2f, groundLayer);
            
            // Jump if there's no ground ahead and we're not beyond a boundary
            // if (!isGroundAhead && !ProximityTrigger2D.IsBeyondBoundary(edgeCheckPosition, new Vector2(direction, 0)))
            // {
            //     shouldJump = true;
            // }
            
            // Try to attack if player is in range
            TryAttackPlayer();
        }
        
        // Handle jumping
        if (shouldJump && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            shouldJump = false;
        }
        base.Update(); // Continue normal behavior when allowed
    }
    
    // Coroutine for initial movement away from door
    private IEnumerator InitialMovement()
    {
        // Wait a moment before starting to move
        yield return new WaitForSeconds(initialMoveDelay);
        
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            
            // Move towards the player
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
            
            Debug.Log("Boss has started moving towards the player");
        }
    }
    
    private void TryAttackPlayer()
    {
        if (player == null) return;
        
        // Check if player is in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Check if cooldown has elapsed
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }
    
    private void Attack()
    {
        Debug.Log("Boss is attacking!");
        
        // Start attack animation
        StartCoroutine(AttackAnimation());
        
        // Check if player is still in range (they might have moved)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Get player health component
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Deal damage to player
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Boss dealt " + attackDamage + " damage to player");
            }
            else
            {
                Debug.LogWarning("Player does not have a Health component");
            }
        }
    }
    
    private IEnumerator AttackAnimation()
    {
        // Store original color
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            // Change color to indicate attack
            spriteRenderer.color = Color.red;
        }
        
        // Pause movement briefly during attack
        float originalSpeed = rb.velocity.x;
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // Wait for attack duration
        yield return new WaitForSeconds(0.3f);
        
        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // Resume movement
        if (!isBlocked)
        {
            float direction = player != null ? Mathf.Sign(player.position.x - transform.position.x) : 0;
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
        }
    }
    
    public void BlockAtBoundaries()
    {
        canPassBoundary = false;
        Debug.Log("Boss can no longer pass boundaries!");
    }

    public void UnlockBoss()
    {
        canMove = true;  // 🔓 Allow movement
        Debug.Log("Boss is now free to move!");
    }

     // Method to check if there is a Candy Cane object in front of the Boss
    private bool IsCandyCaneInFront(GameObject wholeCandyCane)
    {
        // Cast a ray in front of the Boss to check if there is a Candy Cane tagged object
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, candyCaneCheckDistance);
        if (hit.collider != null && wholeCandyCane != null)
        {
            return true;  // Candy Cane object is in front
        }
        return false;  // No Candy Cane in front
    }
     // Method to stop Boss movement and attack behavior
    private void StopBossMovementAndAttack()
    {
        // Disable movement and attack
        rb.velocity = Vector2.zero;  // Stop movement
        lastAttackTime = Time.time;  // Prevent attacks by resetting attack cooldown
        
        Debug.Log("Boss is stopped by Candy Cane!");
    }
} 