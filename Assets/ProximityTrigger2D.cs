// using UnityEngine;
// using System.Collections;

// public class ProximityTrigger2D : MonoBehaviour
// {
//     [Header("Player Detection")]
//     [Tooltip("Tag of the player object")]
//     public string playerTag = "Player";
    
//     [Tooltip("How close the player needs to be to trigger this")]
//     public float triggerDistance = 0.5f;
    
//     [Header("Candy Cane Collection")]
//     [Tooltip("Tag for Candy Cane collectibles")]
//     public string candyCaneTag = "Candy Cane";
    
//     [Tooltip("Object to reveal only if all candy canes are collected")]
//     public GameObject wholeCandyCaneObject;
    
//     [Tooltip("Optional animation time for revealing the object (0 for instant)")]
//     public float revealAnimTime = 0.5f;
    
//     [Header("Boss Settings")]
//     [Tooltip("Reference to the Boss GameObject")]
//     public GameObject bossObject;
    
//     [Tooltip("Whether to show debug visuals")]
//     public bool showDebug = true;
    
//     // Private variables
//     private Transform playerTransform;
//     private bool isPlayerInRange = false;
//     private bool hasTriggered = false;
//     private bool wholeCandyCaneRevealed = false;
    
//     private void Start()
//     {
//         // Find the player by tag
//         GameObject player = GameObject.FindGameObjectWithTag(playerTag);
//         if (player != null)
//         {
//             playerTransform = player.transform;
//         }
//         else
//         {
//             Debug.LogError("ProximityTrigger2D: No object with tag '" + playerTag + "' found!");
//         }
        
//         // Make sure the whole candy cane object starts hidden
//         if (wholeCandyCaneObject != null)
//         {
//             wholeCandyCaneObject.SetActive(false);
//         }
        
//         // Check if Boss object is assigned
//         if (bossObject == null)
//         {
//             Debug.LogError("ProximityTrigger2D: No Boss object assigned!");
//         }
//     }
    
//     private void Update()
//     {
//         // Skip if already triggered or player not found
//         if (playerTransform == null)
//         {
//             Debug.LogError("ProximityTrigger2D: Player transform is null!");
//             return;
//         }
        
//         if (hasTriggered)
//             return;
        
//         // Check distance to player
//         float distance = Vector2.Distance(transform.position, playerTransform.position);
        
//         // Debug distance
//         if (showDebug && Time.frameCount % 60 == 0)
//         {
//             Debug.Log("Distance to player: " + distance + ", Trigger distance: " + triggerDistance);
//         }
        
//         // Player is in range
//         if (distance <= triggerDistance)
//         {
//             if (!isPlayerInRange)
//             {
//                 isPlayerInRange = true;
//                 Debug.Log("Player entered trigger range!");
//                 TriggerActions();
//             }
//         }
//         else
//         {
//             if (isPlayerInRange)
//             {
//                 isPlayerInRange = false;
//                 Debug.Log("Player exited trigger range!");
//             }
//         }
//     }
    
//     private bool AreAllCandyCanesCollected()
//     {
//         // Check if any candy canes still exist in the scene
//         GameObject[] candyCanes = GameObject.FindGameObjectsWithTag(candyCaneTag);
//         int count = candyCanes.Length;
        
//         Debug.Log("Found " + count + " candy canes with tag '" + candyCaneTag + "'");
        
//         // If no candy canes are found with this tag, all have been collected
//         return count == 0;
//     }
    
//     private void TriggerActions()
//     {
//         Debug.Log("Triggering actions!");
//         hasTriggered = true;
        
//         // Always unlock the boss
//         Unlock();
        
//         // Check for candy canes and reveal object only if all collected
//         bool allCandyCanesCollected = AreAllCandyCanesCollected();
//         Debug.Log("All candy canes collected? " + allCandyCanesCollected);
        
//         if (allCandyCanesCollected)
//         {
//             Debug.Log("All candy canes collected! Revealing WholeCandyCane object.");
//             RevealWholeCandyCane();
//             wholeCandyCaneRevealed = true;
//         }
//         else
//         {
//             Debug.Log("Not all candy canes collected. Boss unlocked but WholeCandyCane remains hidden.");
//         }
//     }
    
//     private void RevealWholeCandyCane()
//     {
//         if (wholeCandyCaneObject == null)
//         {
//             Debug.LogError("ProximityTrigger2D: WholeCandyCane object is null!");
//             return;
//         }
        
//         Debug.Log("Revealing WholeCandyCane: " + wholeCandyCaneObject.name);
        
//         if (revealAnimTime <= 0)
//         {
//             // Instantly reveal
//             wholeCandyCaneObject.SetActive(true);
//             Debug.Log("WholeCandyCane instantly revealed!");
//         }
//         else
//         {
//             // Animate the reveal
//             StartCoroutine(AnimateReveal());
//         }
//     }
    
//     private IEnumerator AnimateReveal()
//     {
//         Debug.Log("Starting reveal animation...");
//         wholeCandyCaneObject.SetActive(true);
        
//         // Get renderer component to fade it in
//         Renderer renderer = wholeCandyCaneObject.GetComponent<Renderer>();
        
//         if (renderer != null)
//         {
//             // Start with transparent material
//             Color startColor = renderer.material.color;
//             startColor.a = 0;
//             renderer.material.color = startColor;
            
//             // Gradually increase alpha
//             float elapsedTime = 0;
//             while (elapsedTime < revealAnimTime)
//             {
//                 elapsedTime += Time.deltaTime;
//                 float alpha = Mathf.Clamp01(elapsedTime / revealAnimTime);
                
//                 Color newColor = renderer.material.color;
//                 newColor.a = alpha;
//                 renderer.material.color = newColor;
                
//                 yield return null;
//             }
            
//             // Ensure it ends at full alpha
//             Color endColor = renderer.material.color;
//             endColor.a = 1;
//             renderer.material.color = endColor;
//             Debug.Log("WholeCandyCane reveal animation completed!");
//         }
//         else
//         {
//             Debug.LogWarning("ProximityTrigger2D: No Renderer found on WholeCandyCane object!");
//         }
//     }
    
//     private void Unlock()
//     {
//         if (bossObject == null)
//         {
//             Debug.LogError("ProximityTrigger2D: Boss object is null!");
//             return;
//         }
        
//         Debug.Log("Unlocking Boss: " + bossObject.name);
        
//         // Try to get the boss component and call Unlock
//         Boss boss = bossObject.GetComponent<Boss>();
//         if (boss != null)
//         {
//             boss.UnlockBoss();
//             Debug.Log("Boss unlocked successfully through Boss component!");
//         }
//         else
//         {
//             Debug.LogWarning("ProximityTrigger2D: No Boss component found on bossObject!");
            
//             // Try to call a SendMessage as fallback
//             bossObject.SendMessage("Unlock", SendMessageOptions.DontRequireReceiver);
//             Debug.Log("Sent 'Unlock' message to Boss object as fallback.");
//         }
//     }
    
//     // Make the trigger visible in the scene view
//     private void OnDrawGizmos()
//     {
//         if (!showDebug)
//             return;
            
//         // Draw the trigger range
//         Gizmos.color = isPlayerInRange ? Color.green : Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, triggerDistance);
        
//         // Show activation state
//         if (hasTriggered)
//         {
//             Gizmos.color = Color.blue;
//             Gizmos.DrawWireSphere(transform.position, triggerDistance + 0.2f);
//         }
        
//         // Show candy cane revealed state
//         if (wholeCandyCaneRevealed && Application.isPlaying)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0));
//         }
//     }
// }


using UnityEngine;
using System.Collections;

public class ProximityTrigger2D : MonoBehaviour
{
    public string playerTag = "Player";
    public string candyCaneTag = "Candy Cane";

    void Update()
    {
        if (AllCandyCanesCollected()){
            Destroy(gameObject);
        }
    }

    bool AllCandyCanesCollected(){
        GameObject[] candyCanes = GameObject.FindGameObjectsWithTag(candyCaneTag);
        return candyCanes.Length == 0;
    } 
}


