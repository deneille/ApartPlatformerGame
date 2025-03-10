using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openSpeed = 2f;          // Speed at which the door opens/closes
    public float openAmount = 1f;         // How far the door opens
    public bool openVertically = false;   // Whether the door opens vertically or horizontally
    public float disappearDuration = 0.5f; // How long it takes for the door to disappear
    public Boss boss;
    
    private Vector3 closedPosition;       // The door's starting position
    private Vector3 openPosition;         // The door's open position
    private bool isOpening = false;       // Whether the door is currently opening
    private bool isClosing = false;       // Whether the door is currently closing
    private SpriteRenderer spriteRenderer; // Reference to the sprite renderer
    private BoxCollider2D boxCollider;     // Reference to the box collider
    
    void Start()
    {
        // Store the initial position
        closedPosition = transform.position;
        
        // Calculate the open position based on direction
        if (openVertically)
        {
            openPosition = closedPosition + new Vector3(0, openAmount, 0);
        }
        else
        {
            openPosition = closedPosition + new Vector3(openAmount, 0, 0);
        }
        
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    
    void Update()
    {
        // If the door is opening, move it towards the open position
        if (isOpening)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);
            
            // Check if door is fully open
            if (Vector3.Distance(transform.position, openPosition) < 0.01f)
            {
                isOpening = false;
            }
        }
        
        // If the door is closing, move it towards the closed position
        if (isClosing)
        {
            transform.position = Vector3.MoveTowards(transform.position, closedPosition, openSpeed * Time.deltaTime);
            
            // Check if door is fully closed
            if (Vector3.Distance(transform.position, closedPosition) < 0.01f)
            {
                isClosing = false;
            }
        }
    }
    
    // Method to open the door and make it disappear
    public void OpenDoor()
    {
        isOpening = true;
        isClosing = false;
        StartCoroutine(DisappearAfterOpening());
        Debug.Log("Door is opening and will disappear");
        
        // Notify the boss to start moving
        if (boss != null)
        {
            boss.UnlockBoss(); // Unlocks the boss to move
        }
    }
    
    // Method to make the door reappear and close
    public void CloseDoor()
    {
        StartCoroutine(ReappearAndClose());
    }
    
    // Coroutine to make the door disappear after opening
    private IEnumerator DisappearAfterOpening()
    {
        // Wait until the door is fully open
        while (isOpening)
        {
            yield return null;
        }
        
        // Fade out the door
        if (spriteRenderer != null)
        {
            float elapsedTime = 0;
            Color originalColor = spriteRenderer.color;
            
            while (elapsedTime < disappearDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsedTime / disappearDuration);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            // Make sure the door is fully transparent
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }
        else
        {
            // If no sprite renderer, just disable the GameObject
            gameObject.SetActive(false);
        }
        
        // Disable the collider if it exists
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
        
        Debug.Log("Door has disappeared");
    }
    
    // Coroutine to make the door reappear and close
    private IEnumerator ReappearAndClose()
    {
        // Enable the GameObject if it was disabled
        gameObject.SetActive(true);
        
        // Enable the collider if it exists
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
        
        // Fade in the door
        if (spriteRenderer != null)
        {
            float elapsedTime = 0;
            Color originalColor = spriteRenderer.color;
            
            while (elapsedTime < disappearDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, elapsedTime / disappearDuration);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            // Make sure the door is fully opaque
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
        }
        
        // Start closing the door
        isClosing = true;
        isOpening = false;
        
        Debug.Log("Door has reappeared and is closing");
    }
}