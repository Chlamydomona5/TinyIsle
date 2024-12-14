using UnityEngine;
using UnityEngine.UI;

public class StatButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closeSprite;
    
    private bool _open;
    public void OnClick()
    {
        if(!_open)
        {
            _open = true;
            GridManager.Instance.OpenHeat(60);
            icon.sprite = openSprite;
        }
        else
        {
            _open = false;
            GridManager.Instance.CloseHeat();
            icon.sprite = closeSprite;
        }
    }
}