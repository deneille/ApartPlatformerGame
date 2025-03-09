using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;  

    public HealthUI healthUI;

    public static event Action OnPlayerDeath;

    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        ResetHealth();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameController.OnReset += ResetHealth;
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy){
            TakeDamage(enemy.damage);
        }
    }

    public void TakeDamage(int damage){
        currentHealth -= damage;
        healthUI.UpdateOranges(currentHealth);

        StartCoroutine(FlashRed());
        if(currentHealth <= 0){
            OnPlayerDeath.Invoke();
        }
    }

    void ResetHealth() {
        currentHealth = maxHealth;
        healthUI.SetMaxOranges(maxHealth);
    }

    private IEnumerator FlashRed(){
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}
