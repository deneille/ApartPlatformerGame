using UnityEngine;

public class Player : MonoBehaviour
{
    public float dropDistance = 1f;   // How far the player drops the item
    public GameObject defaultDropItem; // Default item to drop if none collected
    private int collectibles = 0;     // Total collectibles count
    private GameObject lastCollectedItemPrefab;  // Stores the last collected item

    // Add a new variable to track if player is near a proximity trigger
    private bool canDropCandyCane = false;
    private GameObject proximityTarget = null;

    void OnEnable()
    {
        Collectibles.OnItemCollected += AddCollectible;
        GameController.OnReset += ResetCollectibles;
    }

    void OnDisable()
    {
        Collectibles.OnItemCollected -= AddCollectible;
        GameController.OnReset -= ResetCollectibles;
    }

    void Update()
    {
        // Check for X key press with more detailed logging
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("X key pressed! Collectibles count: " + collectibles + ", LastCollectedItemPrefab: " + (lastCollectedItemPrefab != null ? lastCollectedItemPrefab.name : "null"));
            
            if (collectibles > 0)
            {
                // Check if trying to use a Candy Cane
                bool isCandyCane = lastCollectedItemPrefab != null && 
                                  (lastCollectedItemPrefab.name.Contains("Candy Cane") || 
                                   (lastCollectedItemPrefab.name.Contains("candy") && lastCollectedItemPrefab.name.Contains("cane")));
                
                if (isCandyCane)
                {
                    if (canDropCandyCane)
                    {
                        // For Candy Canes, just decrease the counter at the proximity target
                        UseCandyCane();
                    }
                    else
                    {
                        Debug.Log("Cannot use Candy Cane here - not near a valid use zone!");
                    }
                }
                else
                {
                    // For other collectibles, drop them as usual
                    Debug.Log("Attempting to drop a collectible...");
                    DropDistractionItem();
                }
            }
            else
            {
                Debug.Log("No collectibles to use! Collectibles count is " + collectibles);
            }
        }
    }

    void DropDistractionItem()
    {
        // Determine which prefab to use
        GameObject prefabToDrop = lastCollectedItemPrefab != null ? lastCollectedItemPrefab : defaultDropItem;
        Debug.Log("Attempting to drop prefab: " + (prefabToDrop != null ? prefabToDrop.name : "null"));
        
        if (prefabToDrop != null)
        {
            // Get the worth of the item being dropped
            int itemWorth = 1; // Default worth
            Collectibles collectibleInfo = prefabToDrop.GetComponent<Collectibles>();
            if (collectibleInfo != null)
            {
                itemWorth = collectibleInfo.worth;
            }
            
            // Check if player has enough collectibles to drop this item
            if (collectibles < itemWorth)
            {
                Debug.LogWarning("Not enough collectibles to drop this item! Need " + itemWorth + " but only have " + collectibles);
                return;
            }
            
            // Check if this is a Candy Cane
            bool isCandyCane = prefabToDrop.name.Contains("Candy Cane") || 
                              (prefabToDrop.name.Contains("candy") && prefabToDrop.name.Contains("cane"));
            
            Vector2 dropPosition;
            
            // Special handling for Candy Cane items
            if (isCandyCane && proximityTarget != null)
            {
                // Drop at the proximity target's position
                dropPosition = proximityTarget.transform.position;
                Debug.Log("Dropping Candy Cane at proximity target: " + dropPosition);
            }
            else
            {
                // Find the nearest enemy for normal items
                GameObject nearestEnemy = FindNearestEnemy();
                
                if (nearestEnemy != null)
                {
                    // Calculate position between player and enemy
                    Vector2 playerPos = transform.position;
                    Vector2 enemyPos = nearestEnemy.transform.position;
                    
                    // Calculate distance between player and enemy
                    float distanceToEnemy = Vector2.Distance(playerPos, enemyPos);
                    
                    // Position is 1/3 of the way from player to enemy, but not too far
                    float lerpAmount = Mathf.Min(0.33f, 2f / distanceToEnemy);
                    dropPosition = Vector2.Lerp(playerPos, enemyPos, lerpAmount);
                    
                    Debug.Log("Dropping between player and enemy at: " + dropPosition + 
                              ", Distance to enemy: " + distanceToEnemy + 
                              ", Enemy: " + nearestEnemy.name);
                }
                else
                {
                    // If no enemy found, drop in front of player
                    float facingDirection = transform.localScale.x > 0 ? 1 : -1;
                    dropPosition = (Vector2)transform.position + new Vector2(dropDistance * facingDirection, 0);
                    Debug.Log("No enemy found. Dropping in front of player at: " + dropPosition);
                }
            }
            
            try
            {
                // Instantiate the dropped item
                GameObject droppedItem = Instantiate(prefabToDrop, dropPosition, Quaternion.identity);
                droppedItem.SetActive(true);
                Debug.Log("Successfully instantiated dropped item: " + droppedItem.name);
                
                // Make sure the dropped item has the Collectibles component
                Collectibles collectibleComponent = droppedItem.GetComponent<Collectibles>();
                if (collectibleComponent != null)
                {
                    // Mark as a dropped distraction item
                    collectibleComponent.isDroppedDistraction = true;
                    Debug.Log("Marked as dropped distraction item");
                    
                    // Ensure it has a collider for enemy detection
                    CircleCollider2D collider = droppedItem.GetComponent<CircleCollider2D>();
                    if (collider == null)
                    {
                        collider = droppedItem.AddComponent<CircleCollider2D>();
                    }
                    
                    // Set up the collider
                    collider.isTrigger = true;
                    collider.radius = collectibleComponent.distractionRadius / 2;
                    Debug.Log("Set up collider with radius: " + collider.radius);
                    
                    // Immediately call DistractEnemies to notify nearby enemies
                    collectibleComponent.DistractEnemies();
                    
                    // Decrement collectible count by the item's worth
                    collectibles -= itemWorth;
                    
                    // Update progress slider with negative worth
                    UpdateProgressSlider(-itemWorth);
                    
                    Debug.Log("Dropped collectible as distraction. Worth: " + itemWorth + ", Remaining collectibles: " + collectibles);
                    
                    // If no more collectibles left, reset lastCollectedItemPrefab
                    if (collectibles <= 0)
                    {
                        collectibles = 0;
                        lastCollectedItemPrefab = null;
                        Debug.Log("No more collectibles left!");
                    }
                }
                else
                {
                    Debug.LogError("Dropped item doesn't have Collectibles component! This won't work for distraction.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error dropping item: " + e.Message + "\n" + e.StackTrace);
            }
        }
        else
        {
            Debug.LogError("No prefab to drop! Assign a default drop item in the inspector.");
        }
    }

    // Find the nearest enemy to the player
    private GameObject FindNearestEnemy()
    {
        // Find all enemies in the scene
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        
        if (enemies.Length == 0)
            return null;
        
        GameObject nearest = enemies[0].gameObject;
        float minDistance = Vector2.Distance(transform.position, nearest.transform.position);
        
        foreach (Enemy enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.gameObject;
            }
        }
        
        return nearest;
    }

    // Update the progress slider
    private void UpdateProgressSlider()
    {
        // Find the GameController to update the progress slider
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.UpdateProgress(collectibles);
        }
    }

    // Update the progress slider with a specific change
    private void UpdateProgressSlider(int change)
    {
        // Find the GameController to update the progress slider
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.UpdateProgressAmount(change);
        }
    }

    void AddCollectible(GameObject collectiblePrefab, int worth)
    {
        Debug.Log("AddCollectible called with prefab: " + (collectiblePrefab != null ? collectiblePrefab.name : "null") + ", worth: " + worth);
        
        // Increase collectible count by the worth
        collectibles += worth;
        
        // Update progress slider with the worth value
        UpdateProgressSlider(worth);
        
        // Store a reference to the prefab
        if (collectiblePrefab != null)
        {
            lastCollectedItemPrefab = collectiblePrefab;
            Debug.Log("Collected item! Total collectibles: " + collectibles + ", Stored prefab: " + lastCollectedItemPrefab.name);
        }
        else
        {
            Debug.LogWarning("Collected item with null prefab reference!");
        }
    }

    // Method to be called by ProximityTrigger2D when player enters trigger
    public void EnableCandyCaneUsage(GameObject target)
    {
        canDropCandyCane = true;
        proximityTarget = target;
        Debug.Log("Candy Cane usage enabled near: " + (target != null ? target.name : "unknown"));
    }

    // Method to be called by ProximityTrigger2D when player exits trigger
    public void DisableCandyCaneUsage()
    {
        canDropCandyCane = false;
        proximityTarget = null;
        Debug.Log("Candy Cane usage disabled");
    }

    // Method to use Candy Cane at a proximity target without dropping it
    void UseCandyCane()
    {
        if (lastCollectedItemPrefab != null && proximityTarget != null)
        {
            // Get the worth of the Candy Cane
            int itemWorth = 1; // Default worth
            Collectibles collectibleInfo = lastCollectedItemPrefab.GetComponent<Collectibles>();
            if (collectibleInfo != null)
            {
                itemWorth = collectibleInfo.worth;
            }
            
            // Check if player has enough collectibles
            if (collectibles < itemWorth)
            {
                Debug.LogWarning("Not enough collectibles to use this Candy Cane! Need " + itemWorth + " but only have " + collectibles);
                return;
            }
            
            Debug.Log("Using Candy Cane at proximity target: " + proximityTarget.name);
            
            // Activate the target object or perform any special action
            if (proximityTarget.GetComponent<Animator>())
            {
                proximityTarget.GetComponent<Animator>().SetTrigger("Activate");
            }
            
            
            // Decrement collectible count by the item's worth
            collectibles -= itemWorth;
            
            // Update progress slider with negative worth
            UpdateProgressSlider(-itemWorth);
            
            Debug.Log("Used Candy Cane. Worth: " + itemWorth + ", Remaining collectibles: " + collectibles);
            
            // If no more collectibles left, reset lastCollectedItemPrefab
            if (collectibles <= 0)
            {
                collectibles = 0;
                lastCollectedItemPrefab = null;
                Debug.Log("No more collectibles left!");
            }
        }
        else
        {
            Debug.LogError("Cannot use Candy Cane - missing prefab or proximity target!");
        }
    }


    // Reset collectibles when the game is reset
    void ResetCollectibles()
    {
        collectibles = 0;
        lastCollectedItemPrefab = null;
        
        // Reset Candy Cane tracking
        // ProximityTrigger2D.candyCanesCollected = 0;
        
        Debug.Log("Player collectibles reset");
    }
}




