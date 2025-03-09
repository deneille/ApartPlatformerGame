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
    // Start is called before the first frame update
    void Start()
    {
        progressAmount = 0;
        progressSlider.value = 0;
        Collectibles.OnItemCollected += (GameObject obj, int worth) => IncreasedProgressAmount(worth);
        PlayerHealth.OnPlayerDeath += GameOverScreen;
        gameOverScreen.SetActive(false);
    }

    void IncreasedProgressAmount(int amount)
    {
        progressAmount += amount;
        progressSlider.value = progressAmount;
        if (progressAmount >= 100)
        {
            Debug.Log("You Win!");
        }
    
    }

    public void ResetGame() {
        gameOverScreen.SetActive(false);
        OnReset?.Invoke();
        Time.timeScale = 1;
    }


    void GameOverScreen() {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        Collectibles.OnItemCollected -= (GameObject obj, int worth) => IncreasedProgressAmount(worth);
    }
}
