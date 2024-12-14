using UnityEngine;
using UnityEngine.UI;

public class ShopUnfoldButton : MonoBehaviour
{
    [SerializeField] private bool unfold;
    
    [SerializeField] private Sprite foldSprite;
    [SerializeField] private Sprite unfoldSprite;
    
    [SerializeField] private Image image;
    
    public void OnClick()
    {
        unfold = !unfold;
        image.sprite = unfold ? unfoldSprite : foldSprite;
    }
}