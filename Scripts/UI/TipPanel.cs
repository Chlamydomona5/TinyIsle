using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : MonoBehaviour
{
    [SerializeField] private TipUI tipUIPrefab;
    [SerializeField] private Transform tipParent;

    public void TipViaID(string tipID)
    {
        var guideTip = Instantiate(tipUIPrefab, tipParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tipParent.GetComponent<RectTransform>());
        guideTip.Init(Methods.GetLocalText("Tip_" + tipID));
    }
    
    public void TipStraight(string text)
    {
        var guideTip = Instantiate(tipUIPrefab, tipParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tipParent.GetComponent<RectTransform>());
        guideTip.Init(text);
    }

    /*public IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            Tip("1");
        }
    }*/
}