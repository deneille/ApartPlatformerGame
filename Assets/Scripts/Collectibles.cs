using System;
using UnityEngine;
// Add reference to the IItem interface
using System.Collections;
using System.Collections.Generic;

// Remove the duplicate IItem interface
// public interface IItem
// {
//     void Collect();
// }

public interface IDistractionItem
{
    void DistractEnemies();
    Vector2 GetPosition();
}

public class Collectibles : MonoBehaviour, IItem, IDistractionItem
{
    public static event Action<GameObject, int> OnItemCollected;
    public int worth = 1;  // Worth of the collectible
    public float distractionRadius = 5f; // How far enemies can notice this item
    
    // Reference to the prefab that should be used when dropping this item
    public GameObject dropPrefab;

    public void Collect()
    {
        // Pass the drop prefab to the player
        OnItemCollected?.Invoke(dropPrefab, worth);
        Destroy(gameObject);
    }

    public void DistractEnemies()
    {
        // Find all enemies within radius and distract them
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, distractionRadius);
        foreach (Collider2D collider in nearbyColliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.isDistracted = true;
                Debug.Log("Enemy distracted!");
            }
        }
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    private void OnEnable()
    {
        // When dropped as distraction, immediately try to distract enemies
        DistractEnemies();
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the distraction radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distractionRadius);
    }
}

