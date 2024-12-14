using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyUI : MonoBehaviour
{
    [SerializeField] PropertyManager.PropertyType type;
    
    [SerializeField] private List<ProgressBarUI> progressBarUIs;
    
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI avrText;

    private void Start()
    {
        AddLimit(1);
    }

    public void Refresh()
    {
        progressText.text = PropertyManager.Instance.GetPropertyText(type);
        avrText.text = PropertyManager.Instance.AvrIncome(type).ToString("N0") + "/s";
        //If the number is big enough, show the time to next limit
        if(PropertyManager.Instance.currentExpandCost > 100) avrText.text +=
            (" : " + PropertyManager.Instance.TimeToNextLimit());

        if (type == PropertyManager.PropertyType.Gold)
        {
            var left = PropertyManager.Instance.Gold;
            for (int i = 0; i < 5; i++)
            {
                var limit = PropertyManager.Instance.GetExpandCostAfter(i);
                if (left >= limit)
                {
                    progressBarUIs[i].SetTo(1);
                    left -= limit;
                }
                else if (left >= 0f)
                {
                    progressBarUIs[i].SetTo(left / limit);
                    left = -1;
                }
                else
                {
                    progressBarUIs[i].SetTo(0);
                }
            }
        }
    }

    private void TrySetActive(int i ,bool active)
    {
        if(progressBarUIs.Count <= i) return;
        
        if(progressBarUIs[i].gameObject.activeSelf != active)
        {
            progressBarUIs[i].gameObject.SetActive(active);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }

    private void FixedUpdate()
    {
        Refresh();
    }
    
    public void AddLimit(int count)
    {
        var prev = progressBarUIs.Where(x => x.gameObject.activeSelf).Count();
        for (int i = 0; i < count; i++)
        {
            TrySetActive(prev + i, true);
        }
    }
}