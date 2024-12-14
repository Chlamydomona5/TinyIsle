using UnityEngine;
using UnityEngine.UI;

public class WindowScaleButton : MonoBehaviour
{
    [SerializeField] private bool isNormalSize;
    
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite miniSprite;
    
    [SerializeField] private Image image;
    
    public void OnClick()
    {
        isNormalSize = !isNormalSize;
        image.sprite = isNormalSize ? normalSprite : miniSprite;
        CameraManager.Instance.ScaleWindow(isNormalSize ? 20 : 10);
    }
}