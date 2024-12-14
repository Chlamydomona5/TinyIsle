using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class UnitModel : SerializedMonoBehaviour
{
    [Title("通用反馈")]
    [Title("被创建时",bold:false)]
    public MMF_Player createFeedback;
    [Title("生产时（会在Event Feedback处自动触发爆水晶）",bold:false)]
    public MMF_Player produceFeedback;
    [Title("被点击时",bold:false)]
    public MMF_Player clickFeedback;
    
    [Title("当单位被创建时")]
    public UnityEvent onCreate;
    [Title("当单位升级时")]
    public UnityEvent onNormalUpgrade;

    [Title("当单位特性激活时")] 
    public UnityEvent<UnitFeature_SO> onFeatureActivate;

    [Title("单位特性生效时播放的动画（会在Event Feedback处自动触发特性）")] public List<(string featureID, MMF_Player feedback)> OnFeatureEffects = new();
    
    [Title("单位进化需要更换的模型")]
    [SerializeField] private Dictionary<int, GameObject> _evolutionModels = new Dictionary<int, GameObject>();
    public int ModelCount => _evolutionModels.Count;
    public float CurrentHeight => _currentModel ? _currentModel.bounds.size.y : 0;

    private MeshRenderer _currentModel;

    private void Start()
    {
        _currentModel = GetComponentInChildren<MeshRenderer>();
    }

    public void EvolveModel(int level)
    {
        if (_evolutionModels.ContainsKey(level))
        {
            foreach (var model in _evolutionModels)
            {
                model.Value.SetActive(false);
            }
            _evolutionModels[level].SetActive(true);
            _currentModel = _evolutionModels[level].GetComponent<MeshRenderer>();
        }
    }
    
    
    [Title("生产型作物")]
    [Title("当单位生产时")]
    public UnityEvent onProduce;
    
    [Title("消耗型作物")]
    [Title("当单位消耗时")]
    public UnityEvent onConsume;

    [Title("精灵之家")]
    public UnityEvent onSpiritBack;
    public UnityEvent onSpiritOut;
}