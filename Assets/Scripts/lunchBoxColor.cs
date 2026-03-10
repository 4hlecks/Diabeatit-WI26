using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LunchBoxColor : MonoBehaviour
{
    public Image lunchBoxImage;
    public Image coasterImage;

    public Sprite[] lunchboxSprites;
    public Sprite[] coasterSprites;

    // Start is called before the first frame update
    void Start()
    {
        ApplySelectedColor();
    }

    public void ApplySelectedColor()
    {
        int index = sceneData.SelectedLunchboxIndex;
        if (lunchBoxImage != null && lunchboxSprites != null && index >= 0 && index < lunchboxSprites.Length)
            {
                lunchBoxImage.sprite = lunchboxSprites[index];
        }
        else
            {
                Debug.LogWarning("Lunchbox sprite index invalid or not assigned."); 
            }
        if (coasterImage != null && coasterSprites != null && index >= 0 && index < coasterSprites.Length)
            {
                coasterImage.sprite = coasterSprites[index];
        }
        else
            {
                Debug.LogWarning("Coaster sprite index invalid or not assigned."); 
        }
    }
}
