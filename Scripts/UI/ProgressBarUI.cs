using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image progressImage;

    [SerializeField] private bool widthControl;
    [SerializeField, ShowIf("widthControl")] private Image backgroundImage;
    [SerializeField] private Image glowImage;

    public void SetTo(float ratio)
    {
        if(widthControl)
        {
            var size = backgroundImage.rectTransform.sizeDelta;
            size.x *= ratio;
            progressImage.rectTransform.sizeDelta = size;
        }
        else
        {
            progressImage.fillAmount = ratio;
        }
        //Glow when the progress is full
        glowImage?.gameObject.SetActive(ratio > 0.999f);
    }
}