using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoPanel : MonoBehaviour
{
    [Title("References")]
    [SerializeField] private InfoIconSet iconSet;
    
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    
    [SerializeField] private Transform shortInfoParent;
    [SerializeField] private Transform longInfoParent;
    [SerializeField] private Transform buffParent;
    
    [SerializeField] private TextMeshProUGUI descriptionText; 
    [SerializeField] private List<KeyValuePairUI> basicInfos;
    [SerializeField] private List<KeyValuePairUI> specialKeyValueInfos;
    [SerializeField] private List<KeyValuePairUI> specialIconInfos;
    
    [SerializeField] private List<KeyValuePairUI> longInfos;
    
    [SerializeField] private List<KeyValuePairUI> featureInfos;
    
    [SerializeField] private List<BuffUI> buffInfos;
    
    [SerializeField] private Button evolveButton;
    [SerializeField] private TextMeshProUGUI evolveButtonText;
    [SerializeField] private Sprite evolveAbleSprite;
    [SerializeField] private Sprite evolveDisableSprite;

    private UnitEntity _currentEntity;
    private Unit_SO _currentUnitSo;
    
    private Coroutine _refreshCoroutine;

    public void LoadInfoEntity(UnitEntity entity)
    {
        if (_currentEntity != entity)
        {
            //Update when open
            _currentEntity = entity;
            
            nameText.text = entity.unitSo.LocalizeName;
            descriptionText.text = entity.unitSo.Description;

            icon.sprite = entity.unitSo.Icon;
        }
        
        if(shortInfoParent && shortInfoParent.gameObject.activeSelf)
            InitShortInfoEntity(entity);
        else
            InitLongInfoEntity(entity);
        
        InitFeatureInfoEntity(entity);
        
        if(entity is ProduceUnitEntity produceUnitEntity)
            RefreshBuffInfoEntity(produceUnitEntity);

        InitEvolveButton();
        
        //Force update layout
        RebuildLayout();
    }

    private void InitEvolveButton()
    {
        if (_currentEntity.unitSo.evolvable || _currentEntity.IsEvolvedAtMaxLevel())
        {
            evolveButton.gameObject.SetActive(true);

            evolveButton.onClick.RemoveAllListeners();
            evolveButton.onClick.AddListener(() =>
            {
                if(!_currentEntity.CanEvolveNow()) return;
                PropertyManager.Instance.AddProperty(-_currentEntity.XPToUpgrade + _currentEntity.xp);
                _currentEntity.Evolve();
                LoadInfoEntity(_currentEntity);
            });
        } 
        else
        {
            evolveButton.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        UpdateEvolveButton();
    }

    private void UpdateEvolveButton()
    {
        if(!_currentEntity) return;
        var canEvolveNow = _currentEntity.CanEvolveNow();
        evolveButton.interactable = canEvolveNow;
        evolveButton.image.sprite = canEvolveNow ? evolveAbleSprite : evolveDisableSprite;
        evolveButtonText.text = (canEvolveNow || PropertyManager.Instance.Gold < _currentEntity.XPToUpgrade - _currentEntity.xp) ? (_currentEntity.XPToUpgrade - _currentEntity.xp).ToString() : "X";
    }

    private void RefreshBuffInfoEntity(ProduceUnitEntity entity)
    {
        //TODO: Buff display logic
        var buffs = entity.GetBuffs();
        SetActiveCountTo(buffInfos, buffs.Count);
        for (int i = 0; i < buffs.Count; i++)
        {
            buffInfos[i].Init(buffs[i]);
        }
    }
    
    private void InitShortInfoEntity(UnitEntity entity)
    {
        var iconValuePair = entity.GetIconValuePair().Concat(entity.unitSo.GetIconValuePair()).ToArray();

        //Basic Info
        var basicInfo = iconValuePair.Where(x => x.lineNumber == 0).ToList();
        SetActiveCountTo(basicInfos, basicInfo.Count);
        for (int i = 0; i < basicInfo.Count; i++)
        {
            basicInfos[i].Init(iconSet.GetIcon(basicInfo[i].iconId), basicInfo[i].text);
        }
        
        //Special Info
        var specialInfo = iconValuePair.Where(x => x.lineNumber == 1).ToList();
        SetActiveCountTo(specialKeyValueInfos, specialInfo.Count);
        for (int i = 0; i < specialInfo.Count; i++)
        {
            specialKeyValueInfos[i].Init(iconSet.GetIcon(specialInfo[i].iconId), specialInfo[i].text);
        }
        
        //Special Icon Info
        var specialIconInfo = iconValuePair.Where(x => x.lineNumber == 2).ToList();
        SetActiveCountTo(specialIconInfos, specialIconInfo.Count);
        for (int i = 0; i < specialIconInfo.Count; i++)
        {
            specialIconInfos[i].Init(iconSet.GetIcon(specialIconInfo[i].iconId), "");
        }
    }
    
    private void InitLongInfoEntity(UnitEntity entity)
    {
        //Long Info
        var longInfo = entity.GetIconDescPair().Concat(entity.unitSo.GetIconDescPair()).ToList();
        SetActiveCountTo(longInfos, longInfo.Count);
        for (int i = 0; i < longInfo.Count; i++)
        {
            longInfos[i].Init(iconSet.GetIcon(longInfo[i].iconId), longInfo[i].text);
        }
    }
    
    private void InitFeatureInfoEntity(UnitEntity entity)
    {
        var activitatedFeatures = entity.ActivitatedFeatures;
        SetActiveCountTo(featureInfos, activitatedFeatures.Count);

        int i = 0;
        
        for (i = 0; i < activitatedFeatures.Count; i++)
        {
            featureInfos[i].Init(iconSet.GetIcon("unlockFeature"),Methods.GenerateFeatureDescription(activitatedFeatures[i]));
        }
        
        for (int j = i; j < activitatedFeatures.Count; j++)
        {
            featureInfos[j].Init(iconSet.GetIcon("lockFeature"), "......");
        }
    }
    
    public void LoadInfoSO(Unit_SO unit)
    {
        ChangeMode(true);
        
        if (_currentUnitSo && _currentUnitSo != unit)
        {
            _currentUnitSo = unit;
        }
        
        nameText.text = unit.LocalizeName;
        descriptionText.text = unit.Description;
        
        InitLongInfoSO(unit);
        InitFeatureInfoSO(unit);
        
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
    
    private void InitLongInfoSO(Unit_SO unit)
    {
        //Long Info
        var longInfo = unit.GetIconDescPair();
        SetActiveCountTo(longInfos, longInfo.Count);
        for (int i = 0; i < longInfo.Count; i++)
        {
            longInfos[i].Init(iconSet.GetIcon(longInfo[i].iconId), longInfo[i].text);
        }
    }

    private void InitFeatureInfoSO(Unit_SO unit)
    {
        SetActiveCountTo(featureInfos, unit.features.Count);
        var unlockedLevel = UnlockWebManager.Instance.UnlockedEvolves.ContainsKey(unit) ? UnlockWebManager.Instance.UnlockedEvolves[unit] -1 : 0;
        
        int i = 0;
        
        for (i = 0; i < unlockedLevel; i++)
        {
            featureInfos[i].Init(iconSet.GetIcon("unlockFeature"),Methods.GenerateFeatureDescription(unit.features[i]));
        }
        
        for (int j = i; j < unit.features.Count; j++)
        {
            featureInfos[j].Init(iconSet.GetIcon("lockFeature"), "......");
        }
    }

    public void OnClick()
    {
        ChangeMode(!longInfoParent.gameObject.activeSelf);
    }
    
    public void ChangeMode(bool unfold)
    {
        if(longInfoParent)
            longInfoParent.gameObject.SetActive(unfold);
        
        if(shortInfoParent)
            shortInfoParent.gameObject.SetActive(!unfold);
        
        if (_currentEntity)
            LoadInfoEntity(_currentEntity);
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
            if (_currentEntity)
                LoadInfoEntity(_currentEntity);
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