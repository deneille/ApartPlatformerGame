using System.Collections;
using UnityEngine;

public class DistractionItem : MonoBehaviour
{
    public float distractionTime = 2f; // How long the enemy will be distracted
    private bool isDistracting = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // If the enemy collides with the distraction item, it will distract the enemy
        if (other.CompareTag("Enemy") && !isDistracting)
        {
            StartCoroutine(DistractionRoutine(other.gameObject));
        }
    }

    // This coroutine distracts the enemy for the set time
    private IEnumerator DistractionRoutine(GameObject enemy)
    {
        isDistracting = true;
        Enemy enemyScript = enemy.GetComponent<Enemy>();

        // Temporarily change the enemy's behavior (e.g., stop chasing the player)
        enemyScript.IsDistracted = true;

        // Wait for the distraction time
        yield return new WaitForSeconds(distractionTime);

        // Return enemy to normal behavior
        enemyScript.IsDistracted = false;
        //yes
        Destroy(gameObject); // Remove the distraction item after use
    }
}

