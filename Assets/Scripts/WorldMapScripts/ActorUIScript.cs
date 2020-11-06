using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActorUIScript : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI tmpro;
    public GameObject healthContainer;

    public void SetSprite(Sprite sprite)
    {
        image.overrideSprite = sprite;
    }
    public void SetName(string text)
    {
        tmpro.text = text;
    }
    public void SetHealthPercentage(float perc)
    {
        healthContainer.transform.localScale = new Vector3(perc, healthContainer.transform.localScale.y, healthContainer.transform.localScale.z);
    }
}
