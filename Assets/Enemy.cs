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

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
    }
    void Update(){
        Debug.Log("Update called");
        //isGrounded with increased distance and debug ray
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 2f, Color.red); // Visualize ground detection ray
        Debug.Log("isGrounded: " + isGrounded);

        if(player == null) {
            Debug.LogError("Player reference is missing!");
            return;
        }

        //Player Direction
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Debug.Log("Direction: " + direction);

        //Player Above detection
        bool isPlayerAbove = player.CompareTag("Player") && Physics2D.Raycast(transform.position, Vector2.up, 3f, groundLayer);
        Debug.DrawRay(transform.position, Vector2.up * 3f, Color.blue); // Visualize player above detection ray
        Debug.Log("isPlayerAbove: " + isPlayerAbove);

        // Check for edges
        Vector2 edgeCheckStart = (Vector2)transform.position + new Vector2(direction * 0.5f, 0);
        RaycastHit2D edgeCheck = Physics2D.Raycast(edgeCheckStart, Vector2.down, 2f, groundLayer);
        Debug.DrawRay(edgeCheckStart, Vector2.down * 2f, Color.magenta); // Visualize edge detection

        if(isGrounded){
            Debug.Log("Enemy is grounded, attempting to move");
            
            // Only move if there's ground ahead
            if(edgeCheck.collider != null) {
                //Chase player
                rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
                Debug.Log("Setting velocity to: " + new Vector2(direction * chaseSpeed, rb.velocity.y));
            } else {
                // Stop at edge if no ground ahead
                rb.velocity = new Vector2(0, rb.velocity.y);
                Debug.Log("Stopping at edge");
            }

            //jump if there is a gap ahead and no ground in front 
            //else if there's a player on platform above
            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction,0), 2f, groundLayer);
            Debug.DrawRay(transform.position, new Vector2(direction,0) * 2f, Color.green); // Visualize forward detection ray
            Debug.Log("groundInFront: " + groundInFront.collider);

            RaycastHit2D gapAhead = Physics2D.Raycast((Vector2)transform.position + new Vector2(direction, 0), Vector2.down, 2f, groundLayer);
            Debug.DrawRay((Vector2)transform.position + new Vector2(direction, 0), Vector2.down * 2f, Color.yellow); // Visualize gap detection ray
            Debug.Log("gapAhead: " + gapAhead.collider);

            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, groundLayer);
            Debug.Log("platformAbove: " + platformAbove.collider);

            // Modified jump conditions
            if(groundInFront.collider != null && !gapAhead.collider){
                shouldJump = true;
                Debug.Log("Should jump due to obstacle ahead");
            }
            else if(isPlayerAbove && platformAbove.collider && Vector2.Distance(transform.position, player.position) < 5f){
                shouldJump = true;
                Debug.Log("Should jump due to player above and in range");
            }
        } else {
            Debug.Log("Enemy is NOT grounded");
            // Add some air control
            rb.velocity = new Vector2(direction * chaseSpeed * 0.5f, rb.velocity.y);
        }
    }
    void FixedUpdate()
    {
        Debug.Log("FixedUpdate called");
        //Enemy Jumps
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 playerDirection = (player.position - transform.position).normalized;
            Vector2 jumpVelocity = new Vector2(playerDirection.x * jumpForce, jumpForce);

            rb.AddForce(jumpVelocity, ForceMode2D.Impulse);
            Debug.Log("Jumping with velocity: " + jumpVelocity);
        }
    }
}
