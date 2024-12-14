using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpiritInfoPanel : MonoBehaviour
{
    public static float StaminaBarMaxAmount = 20f;
    public static float SpeedBarMaxAmount = 10f;
    public static float MaxLoadBarMaxAmount = 10f;
    
    [Title("References")]
    [SerializeField] private InfoIconSet iconSet;
    
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private ProgressBarUI heartBar;
    [SerializeField] private TextMeshProUGUI heartCount;
    
    [SerializeField] private ProgressBarUI staminaBar;
    [SerializeField] private ProgressBarUI staminaBuffBar;
    [SerializeField] private ProgressBarUI speedBar;
    [SerializeField] private ProgressBarUI speedBuffBar;
    [SerializeField] private ProgressBarUI maxLoadBar;
    [SerializeField] private ProgressBarUI maxLoadBuffBar;
    
    //[SerializeField] private List<KeyValuePairUI> buffInfos;

    private Spirit _currentSpirit;
    
    private Coroutine _refreshCoroutine;

    public void LoadInfo(Spirit spirit)
    {
        if (_currentSpirit != spirit)
        {
            _currentSpirit = spirit;

            icon.sprite = spirit.belongUnit.portageUnitSo.SpiritIcon;
            nameText.text = spirit.belongUnit.portageUnitSo.LocalizeName;

            heartBar.SetTo(spirit.heartProgress);
            heartCount.text = $"x{spirit.currentHeart}";
                
            staminaBar.SetTo(spirit.belongUnit.portageUnitSo.stamina / StaminaBarMaxAmount);
            speedBar.SetTo(spirit.belongUnit.portageUnitSo.speed / SpeedBarMaxAmount);
            maxLoadBar.SetTo(spirit.belongUnit.portageUnitSo.maxLoad / MaxLoadBarMaxAmount);
            
            var staminaBuff = spirit.buffCarrier.BuffList.Sum(buff => buff.Impact.stamina);
            staminaBuffBar.SetTo((staminaBuff + spirit.belongUnit.portageUnitSo.stamina) / StaminaBarMaxAmount);
            var speedBuff = spirit.buffCarrier.BuffList.Sum(buff => buff.Impact.speed);
            speedBuffBar.SetTo((speedBuff + spirit.belongUnit.portageUnitSo.speed) / SpeedBarMaxAmount);
            var maxLoadBuff = spirit.buffCarrier.BuffList.Sum(buff => buff.Impact.maxLoad);
            maxLoadBuffBar.SetTo((maxLoadBuff + spirit.belongUnit.portageUnitSo.maxLoad) / MaxLoadBarMaxAmount);
        }
        
        //Force update layout
        RebuildLayout();
    }
    
    private void RebuildLayout()
    {
        foreach (var layout in GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.transform as RectTransform);
        }
    }

    private void OnEnable()
    {
        _refreshCoroutine = StartCoroutine(RefreshInfo());
    }

    private void OnDisable()
    {
        StopCoroutine(_refreshCoroutine);
    }

    private IEnumerator RefreshInfo()
    {
        while (true)
        {
            if (_currentSpirit)
                LoadInfo(_currentSpirit);
            yield return new WaitForSeconds(1);
        }
    }

    private void SetActiveCountTo<T>(List<T> list, int count) where T : MonoBehaviour
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].gameObject.SetActive(i < count);
        }
    }
}