using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldToLoad : MonoBehaviour
{
    public float holdDuration = 1f;
    public UnityEngine.UI.Image fillCircle;

    private float holdTimer = 0f;
    private bool isHolding = false;

    public static event Action OnHoldComplete;


    // Update is called once per frame
    void Update()
    {
       if(isHolding){
           holdTimer += Time.deltaTime;
           fillCircle.fillAmount = holdTimer / holdDuration;
           if(holdTimer >= holdDuration){
            OnHoldComplete?.Invoke();
            ResetHold();
           }

       } 
    }

    public void OnHold(InputAction.CallbackContext context){
        if(context.started){
            isHolding = true;
        }
        else if(context.canceled){
            ResetHold();
        }
    }

    public void ResetHold(){
        isHolding = false;
        holdTimer = 0f;
        fillCircle.fillAmount = 0f;
    }

}
