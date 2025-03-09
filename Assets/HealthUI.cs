using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class HealthUI : MonoBehaviour
{
    public Image healthPrefab;
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;

    private List<Image> oranges = new List<Image>();

    public void SetMaxOranges(int maxOranges){
        foreach(Image orange in oranges){
            Destroy(orange.gameObject);
        }
        oranges.Clear();

        for(int i =0; i < maxOranges; i++){
            Image newOrange = Instantiate(healthPrefab, transform);
            newOrange.sprite = fullHealthSprite;
            // newOrange.color = Color.yellow;
            oranges.Add(newOrange);
        }
    }

    public void UpdateOranges(int currentHealth){
        for(int i = 0; i < oranges.Count; i++){
            if(i < currentHealth){
                oranges[i].sprite = fullHealthSprite;
                // oranges[i].color = Color.yellow;
            }else{
                oranges[i].sprite = emptyHealthSprite;
                // oranges[i].color = Color.white;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
