using UnityEngine;

public class Player : MonoBehaviour
{
    public float dropDistance = 1f;   // How far the player drops the item
    public GameObject defaultDropItem; // Default item to drop if none collected
    private int collectibles = 0;     // Total collectibles count
    private GameObject lastCollectedItemPrefab;  // Stores the last collected item

    void OnEnable()
    {
        Collectibles.OnItemCollected += AddCollectible;
    }

    void OnDisable()
    {
        Collectibles.OnItemCollected -= AddCollectible;
    }

    void Update()
    {
        // Drop the last collected item only if the player has collectibles
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed!");
            if (collectibles > 0)
            {
                DropDistractionItem();
            }
            else
            {
                Debug.Log("No collectibles to drop!");
            }
        }
    }

    void DropDistractionItem()
    {
        // Determine which prefab to use
        GameObject prefabToDrop = lastCollectedItemPrefab != null ? lastCollectedItemPrefab : defaultDropItem;
        
        if (prefabToDrop != null)
        {
            // Drop the item at an offset position
            Vector2 dropPosition = (Vector2)transform.position + new Vector2(dropDistance, 0);
            GameObject droppedItem = Instantiate(prefabToDrop, dropPosition, Quaternion.identity);
            Debug.Log("Item dropped at position: " + dropPosition);

            collectibles--;  // Reduce collectible count
            
            // If no more collectibles left, reset lastCollectedItemPrefab
            if (collectibles <= 0)
            {
                collectibles = 0;
                lastCollectedItemPrefab = null;
            }
        }
        else
        {
            Debug.LogError("No prefab to drop! Assign a default drop item in the inspector.");
        }
    }

    void AddCollectible(GameObject collectiblePrefab, int worth)
    {
        collectibles += worth;  // Increase collectible count
        
        // Only update the prefab reference if we received a valid one
        if (collectiblePrefab != null)
        {
            lastCollectedItemPrefab = collectiblePrefab;
            Debug.Log("Collected item! Total: " + collectibles);
        }
    }
}




