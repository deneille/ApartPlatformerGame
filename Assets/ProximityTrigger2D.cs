using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityTrigger2D : MonoBehaviour
{

    public GameObject target;
    void Start(){
        target.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure the player has the "Player" tag
        {
            Debug.Log("Player is near the object!");
            target.SetActive(true);
        }
    }
    void Update(){
        
    }
}

