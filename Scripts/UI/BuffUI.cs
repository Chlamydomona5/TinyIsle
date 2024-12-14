using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffUI : MonoBehaviour
{
    [SerializeField] private GameObject shortParent;
    [SerializeField] private Image sourceIcon;
    [SerializeField] private Image effectTypeIcon;
    [SerializeField] private Image effectAmountIcon;
    
    [SerializeField] private GameObject longParent;
    [SerializeField] private TextMeshProUGUI expandText;
    
    public void Init(Buff<ProduceImpact> buff)
    {
        sourceIcon.sprite = buff.Source.unitSo.Icon;
    }

    public void OnClick()
    {
        shortParent.SetActive(!shortParent.activeSelf);
        longParent.SetActive(!longParent.activeSelf);   
    }
}