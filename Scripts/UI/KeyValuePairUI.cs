using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyValuePairUI : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    public void Init(Sprite sprite, string value)
    {
        if (image)
            image.sprite = sprite;
        if (text)
            text.text = value;
    }
}