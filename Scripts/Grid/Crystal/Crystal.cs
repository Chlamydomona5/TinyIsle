using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class Crystal : SerializedMonoBehaviour
{
    public static Dictionary<CrystalType, int> StackLimits = new()
    {
        { CrystalType.Gold, 5 },
        { CrystalType.Expand, 1 },
        { CrystalType.Unlock, 1 },
    };

    public static List<int> SizeToAmountGold = new()
    {
        2, 4, 6, 8, 10
    };
    
    public static List<int> SizeToAmountExpand = new()
    {
        2, 6
    };
    
    public static int GetValueGoldStandard(int size, int ascendLevel)
    {
        return (int)(SizeToAmountGold[size - 1] * Mathf.Pow(2, ascendLevel - 1));
    }
    
    public static int GetValueExpand(int size)
    {
        return SizeToAmountExpand[size - 1];
    }

    public CrystalType type;
    
    [SerializeField] private List<MeshRenderer> expandRenderers;
    
    [SerializeField] private MeshRenderer unlockRenderer;
    
    [SerializeField] private MeshRenderer specialGoldRenderer;
    [SerializeField] private List<MeshRenderer> goldRenderers;

    [SerializeField] private ParticleSystem ascendEffect;
    [SerializeField] private GameObject trailEffect;
    
    public int size;
    public int goldAscendLevel;
    public bool isSpecialValue;
    public int specialValueAmount;

    public int Value
    {
        get
        {
            switch (type)
            {
                case CrystalType.Gold:
                    return isSpecialValue ? specialValueAmount : GetValueGoldStandard(size, goldAscendLevel);
                case CrystalType.Expand:
                    return GetValueExpand(size);
                default:
                    return 0;
            }
            
        }
    }
    
    public Vector2Int coordinate;
    public int stackIndex;
    public bool onAnim;

    public void InitAsGoldStandard(Vector2Int coord, int _size = 1, int _ascendLevel = 1)
    {
        type = CrystalType.Gold;
        coordinate = coord;
        size = _size;
        goldAscendLevel = _ascendLevel;
        
        isSpecialValue = false;
        RefreshModel();
    }
    
    public void InitAsGoldSpecial(Vector2Int coord, int specialValue)
    {
        type = CrystalType.Gold;
        coordinate = coord;
        
        isSpecialValue = true;
        specialValueAmount = specialValue;

        RefreshModel();
    }
 
    public void InitAsExpand(Vector2Int coord, int _size)
    {
        type = CrystalType.Expand;
        coordinate = coord;
        size = _size;
        
        RefreshModel();
    }
    
    public void InitAsUnlock(Vector2Int coord)
    {
        type = CrystalType.Unlock;
        coordinate = coord;
        
        RefreshModel();
    }

    private MeshRenderer RefreshModel()
    {
        transform.localScale = Vector3.one;
        
        MeshRenderer model = null;
        
        goldRenderers.ForEach(x => x.gameObject.SetActive(false));
        expandRenderers.ForEach(x => x.gameObject.SetActive(false));
        unlockRenderer.gameObject.SetActive(false);
        specialGoldRenderer.gameObject.SetActive(false);
        trailEffect.SetActive(false);
        
        if (type == CrystalType.Gold)
        {
            ascendEffect.transform.SetParent(transform.parent);
            trailEffect.SetActive(true);
            
            if (isSpecialValue)
            {
                model = specialGoldRenderer;
                model.gameObject.SetActive(true);
            }
            else
            {
                model = goldRenderers[size - 1];
                model.gameObject.SetActive(true);
        
                var propsBlock = new MaterialPropertyBlock();
                model.GetPropertyBlock(propsBlock);
                propsBlock.SetInt("_CrystalLevel", goldAscendLevel);
                model.SetPropertyBlock(propsBlock);
            }
        }
        else if (type == CrystalType.Expand)
        {
            model = expandRenderers[size - 1];
            model.gameObject.SetActive(true);
            
            model.transform.DOLocalRotate(Vector3.up * 360, 10f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        }
        else if (type == CrystalType.Unlock)
        {
            model = unlockRenderer;
            model.gameObject.SetActive(true);
            
            model.transform.DOLocalRotate(Vector3.up * 360, 10f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        }
        
        return model;
    }
    
    public void TurnOffTrail()
    {
        trailEffect.SetActive(false);
    }

    public void Ascend()
    {
        if (type == CrystalType.Expand) return;

        goldAscendLevel++;
        
        ascendEffect.transform.position = transform.position;
        ascendEffect?.Play();
        RefreshModel();
        
        AchievementManager.Instance.AssignProgress(AchievementType.AscendLevel, goldAscendLevel);
        AchievementManager.Instance.AccumulateProgress(AchievementType.AscendCount);
    }

    private void OnDisable()
    {
        transform.DOKill();
    }
    
    public Tween FlipToAnimation( Vector3 from, Vector2Int des, float time = 0.3f, int index = 0, bool rotate = true)
    {
        onAnim = true;
        var tween = transform.FlipToCoordAnim(from, des, time, index, rotate);
        tween.onComplete += () =>
        {
            onAnim = false;
        };
        return tween;
        
    }
}

[Flags]
public enum CrystalType
{
    Nothing = 0,
    Gold = 1,
    Unlock = 2,
    Expand = 4,
}