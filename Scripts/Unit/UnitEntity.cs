using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DamageNumbersPro;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitEntity : EntityBase, IInfoDisplay, ISaveable
{
    [Title("Level")] public int evolveLevel;
    public int xp;
    public int XPToUpgrade => (int)unitSoRef.cost * (int)Mathf.Pow(10, evolveLevel - 1);

    [ReadOnly] public UnitModel model;
    [ReadOnly] public Unit_SO unitSo;
    [ReadOnly] public Unit_SO unitSoRef;

    [SerializeField] private DamageNumber levelNumberPrefab;
    
    public virtual string UniqueAttributeInfo => "";

    [Title("Features")] [SerializeField, ReadOnly]
    protected List<UnitFeature_SO> activitatedFeatures = new();
    protected Dictionary<UnitFeature_SO, float> featureTimers = new();
    
    
    private Coroutine _featureTimer;
    private Coroutine _refreshDisableSignCoroutine;
    private DamageNumber levelNumberInstance;
    
    public List<UnitFeature_SO> ActivitatedFeatures => activitatedFeatures;
    
    private MMF_Player _evolveVFX;
    
    public int GetCurrentMaxEvolveLevel()
    {
        return UnlockWebManager.Instance.UnlockedEvolves.TryGetValue(unitSoRef, out var level) ? level : 1;
    }

    public virtual void Init(Unit_SO info, Vector2Int coord)
    {
        base.Init(coord, info.coveredCoords);
        name = info.ID;
        unitSoRef = info;
        unitSo = Instantiate(info);
        
        //Instantiate model
        var modelInstance = Instantiate(info.model, transform);
        this.model = modelInstance;
        //Get model height
        GetHeight(modelInstance.gameObject);
        //Set model position
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.createFeedback?.PlayFeedbacks();

        //Change All children's layer to "unit"
        gameObject.layer = LayerMask.NameToLayer("Unit");
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Unit");
        }

        MoveTo(coord, 0, false);
        this.model.onCreate.Invoke();

        OnSignDetect.AddListener(DifferentGroundDetect);
    }

    private void DifferentGroundDetect()
    {
        //TODO:
        //if(GridManager.Instance.GetGroundType(coordinate) != unitSo.groundType) SetSign("BadGround", true);
    }

    public virtual void OnEnable()
    {
        _featureTimer = StartCoroutine(FeatureTimer());
        _refreshDisableSignCoroutine = StartCoroutine(SignDetector());
    }

    public virtual void OnDisable()
    {
        StopCoroutine(_featureTimer);
        StopCoroutine(_refreshDisableSignCoroutine);
    }

    public override void MoveTo(Vector2Int coord, float yOffset = 0, bool triggerFeature = true)
    {
        //Move
        transform.DOKill();

        base.MoveTo(coord, yOffset, triggerFeature);

        if (unitSo is ConsumeUnit_SO consume)
        {
            var type = consume.typeMask;
            foreach (var coveredCoord in RealCoveredCoords)
            {
                if (GridManager.Instance.crystalController.HasCrystal(coveredCoord, type))
                {
                    var stack = GridManager.Instance.crystalController.PopCrystalStack(coveredCoord);
                    foreach (var crystal in stack.Crystals)
                    {
                        GridManager.Instance.crystalController.PushCrystalTo(coveredCoord, crystal);
                    }
                }
                // Else Squeeze them
                else
                {
                    GridManager.Instance.crystalController.SqueezeCrystalAt(RealCoveredCoords);
                }

            }
        }
        else if (!unitSo.crystalCoexistable)
            GridManager.Instance.crystalController.SqueezeCrystalAt(RealCoveredCoords);

        if (triggerFeature)
            foreach (var feature in activitatedFeatures)
            {
                feature.OnMove(this);
            }
    }

    public bool CanEvolveNow()
    {
        if (unitSo.evolvable == false) return false;
        if (TestManager.Instance.freeEvolve) return true;
        if (PropertyManager.Instance.Gold < XPToUpgrade - xp) return false;

        return IsEvolvedAtMaxLevel();
    }

    public bool IsEvolvedAtMaxLevel()
    {
        var able = UnlockWebManager.Instance.UnlockedEvolves.ContainsKey(unitSoRef) && evolveLevel < UnlockWebManager.Instance.UnlockedEvolves[unitSoRef];
        if (unitSo.randomActivate) able = evolveLevel < unitSo.randomEvolveLimit;

        return able;
    }

    public void AddXP(int amount)
    {
        if (unitSo.evolvable == false) return;

        if (amount == 0) return;

        xp += amount;
        if (xp >= XPToUpgrade)
        {
            while (xp >= XPToUpgrade)
            {
                xp -= XPToUpgrade;
                Evolve();
            }
        }

        VFXManager.Instance.PlayVFX("AddXP", transform.position);
    }

    protected IEnumerator FeatureTimer()
    {
        foreach (var feature in activitatedFeatures)
        {
            featureTimers[feature] = 0;
        }

        yield return null;
        while (true)
        {
            //Feature interval counting
            foreach (var feature in activitatedFeatures)
            {
                if (!feature.effectTrigger.HasFlag(EffectTrigger.Interval)) continue;

                if (featureTimers[feature] >= feature.effect.GetFloat(FeatureEffect.EffectStatFloat.Interval))
                {
                    featureTimers[feature] = 0;
                    feature.OnFixedInterval(this);
                }
                else
                {
                    featureTimers[feature] += Time.fixedDeltaTime;
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }
    
    public virtual void SetEvolveLevel(int level)
    {
        evolveLevel = level;
        model.EvolveModel(evolveLevel);
        EvolveFeature();
    }

    public virtual bool Evolve()
    {
        evolveLevel++;

        if (evolveLevel > 1)
        {
            _evolveVFX = VFXManager.Instance.PlayVFX("Evolve", transform.position + Vector3.up).GetComponent<MMF_Player>();
            AchievementManager.Instance.AccumulateProgress(AchievementType.EvolveCount);
            _evolveVFX.GetComponent<Spirit_Upgrade>().onFinish.AddListener(() =>
            {
                model.EvolveModel(evolveLevel);
            });
        }
        else
        {
            model.EvolveModel(evolveLevel);
        }

        foreach (var feature in activitatedFeatures)
        {
            feature.OnEvolve(this, evolveLevel);
        }
        
        EvolveFeature();
        return true;
    }

    private void EvolveFeature()
    {
        //Activate features of this level
        if (unitSo.randomActivate)
        {
            ActivateFeature(unitSo.features[Random.Range(0, unitSo.features.Count)]);
        }
        else
        {
            foreach (var featureSo in unitSo.features)
            {
                if (featureSo.unlockEvolveLevel == evolveLevel)
                {
                    ActivateFeature(featureSo);
                }
            }
        }
    }

    protected virtual void OnDestroy()
    {
        foreach (var feature in activitatedFeatures)
        {
            feature.effect?.BeforeDestroy(feature, this);
        }

        GridManager.Instance.RemoveUnit(this);

        GridManager.Instance.onTap.RemoveListener(OnTap);
    }

    public virtual void OnCrystalIn()
    {
        foreach (var feature in activitatedFeatures)
        {
            feature.OnCrystalIn(this);
        }
    }

    public virtual bool Sold()
    {
        var price = unitSo.cost * unitSo.sellPriceConstant * Constant.BasicSellConstant;

        PropertyManager.Instance.AddProperty(price);
        PropertyManager.Instance.RedoCost(unitSo);
        
        VFXManager.Instance.ProduceHint(transform.position, price);
        VFXManager.Instance.PlayVFX("Sell", transform.position);
        
        GridManager.Instance.onUnitSoldDestroy.Invoke(this);
        Destroy(gameObject);
        return true;
    }

    public virtual bool BreakDestroy()
    {
        if (unitSo.only) return false;

        GridManager.Instance.onUnitBreakDestroy.Invoke(this);
        GridManager.Instance.RemoveUnit(this);
        Destroy(gameObject);
        return true;
    }

    protected void ActivateFeature(UnitFeature_SO feature)
    {
        var featureInstance = Instantiate(feature);
        
        activitatedFeatures.Add(featureInstance);
        featureTimers.Add(featureInstance, 0);

        featureInstance.OnActivate(this);

        model.onFeatureActivate.Invoke(featureInstance);
    }

    public override void OnTap()
    {
        base.OnTap();
        model.clickFeedback?.PlayFeedbacks();
    }

    public void ShowLevel(bool show)
    {
        if(!unitSo.evolvable) return;

        if (show)
            levelNumberInstance = levelNumberPrefab.Spawn(transform.position + Vector3.up * (modelHeight + 2f), CanEvolveNow() ? xp + "/" + XPToUpgrade : "Max");
        else
        {
            if (levelNumberInstance) levelNumberInstance.DestroyDNP();
        }
    }

    public virtual List<(string iconId, string text, int lineNumber)> GetIconValuePair()
    {
        var result = new List<(string iconId, string text, int lineNumber)>();
        
        return result;
    }

    public virtual List<(string iconId, string text)> GetIconDescPair()
    {
        var result = new List<(string iconId, string text)>();
        
        if (unitSo.evolvable)
        {
            result.Add(("level", $"进化等级: {evolveLevel}，当前最高可达{GetCurrentMaxEvolveLevel()}级"));
        }
        
        return result;
    }

    public virtual void Save()
    {
        ES3.Save($"Unit_{coordinate.x}_{coordinate.y}.unitSoRef", unitSoRef);
        ES3.Save($"Unit_{coordinate.x}_{coordinate.y}.evolveLevel", evolveLevel);
    }

    public virtual void Load()
    {
        evolveLevel = ES3.Load($"Unit_{coordinate.x}_{coordinate.y}.evolveLevel", evolveLevel);
        SetEvolveLevel(evolveLevel);
    }
}