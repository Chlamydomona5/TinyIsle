using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageButton : MonoBehaviour
{
    public Button button;
    [SerializeField] Image image;
    [SerializeField] private Sprite pageSelectedSprite;
    [SerializeField] private Sprite pageUnselectedSprite;
    [SerializeField] private TextMeshProUGUI text;

    public void Init(int index)
    {
        text.text = (index + 1).ToString();
    }
    
    public void BeSelected(bool selected)
    {
        image.sprite = selected ? pageSelectedSprite : pageUnselectedSprite;
    }
}