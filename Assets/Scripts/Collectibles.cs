using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectibles : MonoBehaviour, IItem
{
    public static event Action<int> OnItemCollected;
    public int worth = 5;

    public void Collect()
    {
        OnItemCollected?.Invoke(worth);
        Destroy(gameObject);
    }

}
