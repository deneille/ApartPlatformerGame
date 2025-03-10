using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    int progressAmount;
    public Slider progressSlider;
    public GameObject gameOverScreen;
    public static event Action OnReset;

    public GameObject player;
    public GameObject LoadCanvas;
    public List<GameObject> levels;

    private int currentLevelIndex = 0;
    bool isResetting = false;
    private static GameController instance;

    void Awake()
    {
        // Singleton pattern to ensure only one GameController exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Ensure game over screen is hidden immediately when scene loads
        HideGameOverScreen();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        progressAmount = 0;
        if (progressSlider != null)
        {
            progressSlider.value = 0;
        }

        // Subscribe to events
        Collectibles.OnItemCollected += (GameObject obj, int worth) => IncreasedProgressAmount(worth);
        PlayerHealth.OnPlayerDeath += ShowGameOverScreen;
        
        // Make absolutely sure game over screen is inactive at start
        HideGameOverScreen();
        isResetting = false;

        HoldToLoad.OnHoldComplete += LoadNextLevel;
        LoadCanvas.SetActive(false);   
        
        // Force update UI state after a short delay
        StartCoroutine(DelayedUICheck());

    }
    
    // Helper method to hide game over screen
    private void HideGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
            Debug.Log("Game over screen hidden");
        }
        else
        {
            Debug.LogError("Game Over Screen is not assigned in the inspector!");
        }
    }
    
    IEnumerator DelayedUICheck()
    {
        // Wait for a short time to ensure UI is properly initialized
        yield return new WaitForSeconds(0.1f);
        
        // Double-check that game over screen is hidden
        if (gameOverScreen != null)
        {
            if (gameOverScreen.activeSelf)
            {
                Debug.LogWarning("Game over screen was still active - forcing it to hide");
                gameOverScreen.SetActive(false);
            }
            else
            {
                Debug.Log("Confirmed game over screen is hidden");
            }
        }
    }

    void IncreasedProgressAmount(int amount)
    {
        progressAmount += amount;
        if (progressSlider != null)
        {
            progressSlider.value = progressAmount;
        }
        if (progressAmount >= 100)
        {
            LoadCanvas.SetActive(true);
            Debug.Log("Level Completed!");
        }
    }

    public void ResetGame() 
    {
        Debug.Log("Reset game called");
        HideGameOverScreen();
        OnReset?.Invoke();
        Time.timeScale = 1;
        isResetting = false;
    }

    void ShowGameOverScreen() 
    {
        Debug.Log("Show game over screen called");
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        Collectibles.OnItemCollected -= (GameObject obj, int worth) => IncreasedProgressAmount(worth);
        PlayerHealth.OnPlayerDeath -= ShowGameOverScreen;
    }

    // Update the progress amount by a specific value
    public void UpdateProgressAmount(int amount)
    {
        progressAmount += amount;
        
        // Ensure progress doesn't go below 0
        if (progressAmount < 0)
            progressAmount = 0;
        
        // Ensure progress doesn't exceed 100
        if (progressAmount > 100)
            progressAmount = 100;
        
        // Update the slider
        if (progressSlider != null)
        {
            progressSlider.value = progressAmount;
            Debug.Log("Updated progress slider to: " + progressAmount + " (change: " + amount + ")");
        }
        
        // Check for win condition
        if (progressAmount >= 100)
        {
            Debug.Log("You Win!");
            // You could trigger a win screen or other win condition here
        }
    }

    // Set the progress to a specific value
    public void UpdateProgress(int value)
    {
        progressAmount = value;
        
        // Ensure progress doesn't go below 0
        if (progressAmount < 0)
            progressAmount = 0;
        
        // Update the slider
        if (progressSlider != null)
        {
            progressSlider.value = progressAmount;
            Debug.Log("Set progress slider to: " + progressAmount);
        }
        
        // Check for win condition
        if (progressAmount >= 100)
        {
            Debug.Log("You Win!");
        }
    }

    void LoadNextLevel(){
        int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
        LoadCanvas.SetActive(false);

        levels[currentLevelIndex].gameObject.SetActive(false);
        levels[nextLevelIndex].gameObject.SetActive(true);

        player.transform.position = new Vector3(0f, 0f, 0f);

        currentLevelIndex = nextLevelIndex;
        progressAmount = 0;
        progressSlider.value = progressAmount;
    }
}
