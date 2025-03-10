using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ApartPlatformer;

// Remove the duplicate IDistractionItem interface since we're using the one from ApartPlatformer namespace
// public interface IDistractionItem
// {
//     void DistractEnemies();
//     Vector2 GetPosition();
// }

public class Collectibles : MonoBehaviour, IItem, IDistractionItem
{
    public static event Action<GameObject, int> OnItemCollected;
    public int worth = 1;  // Worth of the collectible
    public float distractionRadius = 5f; // How far enemies can notice this item
    public float destroyAfterSeconds = 4f; // Time before the dropped item disappears
    
    // Reference to the prefab that should be used when dropping this item
    public GameObject dropPrefab;
    
    // Flag to indicate if this is a dropped item (for distraction)
    [HideInInspector]
    public bool isDroppedDistraction = false;

    void Awake()
    {
        // If no drop prefab is assigned, we'll use a runtime copy
        if (dropPrefab == null)
        {
            Debug.LogWarning("No drop prefab assigned to " + gameObject.name + ". Will create a runtime copy if needed.");
        }
    }

    public void Collect()
    {
        // Only collectible if not a dropped distraction
        if (!isDroppedDistraction)
        {
            Debug.Log("Collecting item: " + gameObject.name + ", Worth: " + worth + ", Has dropPrefab: " + (dropPrefab != null));
            
            // Check if this is a Candy Cane
            bool isCandyCane = gameObject.name.Contains("Candy Cane") || 
                              (gameObject.name.Contains("candy") && gameObject.name.Contains("cane"));
            
            // If it's a Candy Cane, increment the collected count
            // if (isCandyCane)
            // {
            //     ProximityTrigger2D.candyCanesCollected++;
            //     Debug.Log("Candy Cane collected! Total: " + ProximityTrigger2D.candyCanesCollected + 
            //               "/" + ProximityTrigger2D.totalCandyCanesInLevel);
            // }
            
            // Make sure we have a valid drop prefab
            GameObject prefabToUse = dropPrefab;
            if (prefabToUse == null)
            {
                // Create a runtime copy to use as the drop prefab
                prefabToUse = Instantiate(gameObject);
                prefabToUse.name = gameObject.name + "_Copy";
                prefabToUse.SetActive(false);
                DontDestroyOnLoad(prefabToUse);
                Debug.Log("Created runtime copy to use as drop prefab: " + prefabToUse.name);
            }
            
            // Pass the drop prefab to the player
            OnItemCollected?.Invoke(prefabToUse, worth);
            Debug.Log("OnItemCollected event invoked with prefab: " + prefabToUse.name);
            
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Cannot collect this item as it's a dropped distraction");
        }
    }

    public void DistractEnemies()
    {
        Debug.Log("DistractEnemies called on " + gameObject.name);
        
        // Find all enemies within radius and distract them
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, distractionRadius);
        bool distractedAnyEnemy = false;
        
        foreach (Collider2D collider in nearbyColliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Set the enemy's distraction state and target
                enemy.isDistracted = true;
                enemy.SetDistractionTarget(transform.position);
                distractedAnyEnemy = true;
                Debug.Log("Enemy " + enemy.gameObject.name + " distracted by " + gameObject.name);
            }
        }
        
        // If no enemies were found in the initial check, try a direct search
        if (!distractedAnyEnemy)
        {
            // Find all enemies in the scene and check if they're within range
            Enemy[] allEnemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in allEnemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= distractionRadius)
                {
                    // Set the enemy's distraction state and target
                    enemy.isDistracted = true;
                    enemy.SetDistractionTarget(transform.position);
                    distractedAnyEnemy = true;
                    Debug.Log("Enemy " + enemy.gameObject.name + " distracted by direct search");
                }
            }
        }
        
        if (distractedAnyEnemy)
        {
            // Start the self-destruct timer when used as a distraction
            StartCoroutine(SelfDestructAfterDelay());
            Debug.Log("Distraction successful, self-destruct timer started");
        }
        else
        {
            Debug.Log("No enemies found to distract within radius: " + distractionRadius);
        }
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    private void OnEnable()
    {
        // When dropped as distraction, immediately try to distract enemies
        if (isDroppedDistraction)
        {
            Debug.Log("Item dropped, attempting to distract enemies");
            DistractEnemies();
        }
    }
    
    // Coroutine to destroy the collectible after a delay
    private IEnumerator SelfDestructAfterDelay()
    {
        yield return new WaitForSeconds(destroyAfterSeconds);
        Debug.Log("Collectible self-destructing after " + destroyAfterSeconds + " seconds");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the distraction radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distractionRadius);
    }
}

