using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Unit;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProduceUnitEntity : UnitEntity
{
    [Title("Parameters")] 
    public BuffCarrier<ProduceImpact> buffCarrier = new();

    [Title("Data")] 
    [ReadOnly] public ProduceUnit_SO produceUnitSo;
    [ReadOnly] public ProduceUnit_SO produceUnitSoRef;
    
    [Title("Record")] private List<ProduceRecord> _records = new List<ProduceRecord>();
    [SerializeField, ReadOnly] public long produceSum;
    [SerializeField] private float maxTimeRecordOnUnit = 100f;
    [SerializeField] private float maxCubeHeight = 10f;

    [SerializeField] private Transform heatCude;
    [SerializeField] private Transform disableSign;
    [SerializeField] private GameObject fullParticle;

    private int _tiredCount;
    private bool IsTired => _tiredCount >= produceUnitSo.tiredProduceTimes;
    private float _tiredTimer;

    private bool _grown;
    private float _growTimer = 0;
    private int _currentGrowLevel = 0;
    
    [ShowInInspector, ReadOnly] private float _produceTimer;
    private Coroutine _mainCoroutine;

    public float IntervalReal
    {
        get
        {
            /********************************************************/
            /*Interval formula: P = (Self_Interval + Buff_Amount) * Buff_Multiplier*/
            //Interval are clamped to 0.1s
            /********************************************************/
            var interval = produceUnitSo.produceInterval;
            foreach (var buff in buffCarrier.BuffList)
            {
                interval += buff.Impact.intervalAmount;
            }

            float intervalMulitplier = 1;
            foreach (var buff in buffCarrier.BuffList)
            {
                intervalMulitplier *= (1 + buff.Impact.intervalMultiplier);
            }

            interval *= intervalMulitplier;
            interval = Mathf.Clamp(interval, 0.1f, float.MaxValue);
            return interval;
        }
    }

    public int CrystalSize => Mathf.Min(produceUnitSo.crystalSize + buffCarrier.BuffList.Sum(x => x.Impact.addSize), 5);
    public int AscendLevel => Mathf.Min(produceUnitSo.ascendLevel + buffCarrier.BuffList.Sum(x => x.Impact.addAscend), 5);
    public float CritPoss => Mathf.Min(produceUnitSo.critPossibility + buffCarrier.BuffList.Sum(x => x.Impact.critChanceRatio), 1);
    public int ProduceCount => produceUnitSo.produceCount + buffCarrier.BuffList.Sum(x => x.Impact.produceCount);

    public override string UniqueAttributeInfo => $"Produce: TODO: / {IntervalReal:F1}s";

    public override void Init(Unit_SO info, Vector2Int coord)
    {
        base.Init(info, coord);
        //Create SO and Feature Instances
        produceUnitSoRef = (ProduceUnit_SO)unitSoRef;
        produceUnitSo = (ProduceUnit_SO)unitSo;
        var list = new List<UnitFeature_SO>();
        foreach (var featureSo in produceUnitSoRef.features)
        {
            list.Add(Instantiate(featureSo));
        }
        produceUnitSo.features = list;

        //Find Unity Event
        model.produceFeedback?.GetFeedbackOfType<MMF_Events>().PlayEvents.AddListener(() =>
        {
            TryProduce();
        });

        _mainCoroutine = produceUnitSo.produceType == ProduceType.Interval
            ? StartCoroutine(Produce())
            : StartCoroutine(Grow());
        
        StartCoroutine(DetectDisableSign());

        if (produceUnitSo.willTired)
        {
            fullParticle.SetActive(true);
            fullParticle.transform.position = transform.position + Vector3.up * modelHeight / 2f;
            StartCoroutine(TireDown());
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if(_mainCoroutine == null && produceUnitSo)
            _mainCoroutine = produceUnitSo.produceType == ProduceType.Interval
                ? StartCoroutine(Produce())
                : StartCoroutine(Grow());
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        StopCoroutine(_mainCoroutine);
        _mainCoroutine = null;
    }
    
    private IEnumerator Grow()
    {
        var maxGrowCount = model.ModelCount;
        
        yield return null;
        
        while (true)
        {
            if (!_grown)
            {
                _growTimer += Time.fixedDeltaTime;
                
                if (_growTimer >= IntervalReal / maxGrowCount * _currentGrowLevel)
                {
                    _currentGrowLevel = Mathf.Min(_currentGrowLevel + 1, maxGrowCount);
                    GrowModel(_currentGrowLevel);
                
                    //Produce
                    if (_currentGrowLevel >= maxGrowCount)
                    {
                        _grown = true;
                    }
                }
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    public bool TryHarvest()
    {
        if (_grown)
        {
            _grown = false;
            _growTimer = 0;
            _currentGrowLevel = 0;
            GrowModel(_currentGrowLevel);
            
            if (model.produceFeedback)
            {
                if(!model.produceFeedback.IsPlaying)
                    model.produceFeedback.PlayFeedbacks();
            }
            else TryProduce();
            
            foreach (var feature in activitatedFeatures)
            {
                feature.OnHarvest(this);
            }

            return true;
        }
        return false;
    }

    private IEnumerator Produce() 
    {
        _produceTimer = 0;
        yield return null;
        
        while (true)
        {
            if(produceUnitSoRef.produce)
            {
                //Produce
                _produceTimer += Time.fixedDeltaTime;

                if (_produceTimer >= IntervalReal)
                {
                    _produceTimer = 0;

                    if (model.produceFeedback)
                    {
                        if(!model.produceFeedback.IsPlaying)
                            model.produceFeedback.PlayFeedbacks();
                    }
                    else TryProduce();
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
    private IEnumerator TireDown()
    {
        while (true)
        {
            _tiredTimer += Time.fixedDeltaTime;
            if (_tiredTimer >= produceUnitSo.tiredDuration)
            {
                _tiredTimer = 0;
                _tiredCount = Mathf.Max(0, _tiredCount - 1);
                
                if(_tiredCount == 0) fullParticle.SetActive(true);
                Tire(false);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void TryProduce(bool countTired = true)
    {
        if(IsTired) return;
        
        //Produce multiple times
        bool success = ProduceCrystal();
        
        if(success)
            foreach (var feature in activitatedFeatures)
            {
                feature.OnProduce(this, Crystal.SizeToAmountGold[CrystalSize]);
            }
        
        var tired = IsTired;
        if (countTired && produceUnitSo.willTired)
        {
            _tiredCount++;
            fullParticle.SetActive(false);
        }
        //If tired is triggered just now
        if(IsTired && !tired) Tire(true);
        
    }

    public IEnumerator DetectDisableSign()
    {
        while (true)
        {
            if (FindProducePlace().x == -1000 && produceUnitSo.produce &&
                produceUnitSo.produceType == ProduceType.Interval) 
            {
                SetSign("Disable", true);
            }
            else SetSign("Disable", false);
            
            yield return new WaitForSeconds(1);
        }
    }

    public bool ProduceCrystal()
    {
        for (int i = 0; i < ProduceCount; i++)
        {
            Vector2Int des = FindProducePlace();
            if (des.x == -1000) return false;

            if (Random.value < CritPoss)
            {
                for (int j = 0; j < 2; j++)
                {
                    ProduceOnceCrystal(des);
                }
                
                VFXManager.Instance.PlayVFX("Crit", transform.position + Vector3.up * modelHeight / 2f);
            }
            else 
            {
                ProduceOnceCrystal(des);
            }

            //Record    
            produceSum += (int)Crystal.GetValueGoldStandard(CrystalSize, AscendLevel);

            _records.Add(new ProduceRecord()
            {
                amount = Crystal.GetValueGoldStandard(CrystalSize, AscendLevel),
                time = DateTime.Now
            });
            _records.RemoveAll(r => (DateTime.Now - r.time).TotalSeconds > maxTimeRecordOnUnit);
        }
        
        model.onProduce.Invoke();
        return true;

        void ProduceOnceCrystal(Vector2Int des)
        {
            if (produceUnitSo.crystalType == CrystalType.Gold)
                GridManager.Instance.crystalController.GenerateGoldCrystalStandard(transform.position, des,
                    CrystalSize, AscendLevel, true, 0.2f);
            else GridManager.Instance.crystalController.GenerateExpandCrystal(transform.position, des, 1, false, 0);
        }
    }

    public Vector2Int FindProducePlace()
    {
        Vector2Int des;
        if (Random.value < produceUnitSo.toCorePossiblity)
        {
            des = new Vector2Int(1, 1);
        }
        else if(produceUnitSo.distance != 0)
        {
            des = GridManager.Instance.crystalController.FindBestCrystalableByDistance(coordinate, produceUnitSo.distance);
        }
        else
        {
            des = GridManager.Instance.crystalController.FindBestCrystalableAround(this, coordinate);
        }

        return des;
    }

    public void Tire(bool on)
    {
        SetSign("Tired", on);
    }
    
    public override void OnTap()
    {
        base.OnTap();
        foreach (var feature in activitatedFeatures)
        {
            feature.OnTap(this);
        }
    }

    public override bool BreakDestroy()
    {
        var success = base.BreakDestroy();
        foreach (var feature in activitatedFeatures)
        {
            feature.OnBreakDestroy(this);
        }
        return success; 
    }

    public override bool Sold()
    {
        var sold = base.Sold();
        foreach (var feature in activitatedFeatures)
        {
            feature.OnUnitSoldDestroy(this);
        }

        return sold;
    }

    public long HeatSum(int duration)
    {
        long sum = 0;
        foreach (var record in _records.FindAll(r => (DateTime.Now - r.time).TotalSeconds < duration))
        {
            sum += (long)record.amount;
        }

        return sum;
    }

    public void SetHeat(int duration, long maxHeat, bool all)
    {
        if(maxHeat == 0) return;
        heatCude.gameObject.SetActive(true);
        var ratio = ((float)(all ? produceSum : HeatSum(duration))) / maxHeat;
        heatCude.DOScaleY(ratio * maxCubeHeight, 0.5f);
    }

    public void CloseHeat()
    {
        heatCude.gameObject.SetActive(false);
    }
    
    public override bool Evolve()
    {
        if (evolveLevel > 1)
        {
            produceUnitSo.produceInterval = Methods.UpgradeFormula_Interval(evolveLevel, produceUnitSo.produceInterval, produceUnitSoRef.produceIntervalGrowthConstant);
        }
        
        return base.Evolve();
    }

    public void GrowModel(int growLevel)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScaleY(.2f, .1f));
        seq.AppendCallback(() =>
        {
            model.EvolveModel(growLevel);
        });
        seq.Append(transform.DOScaleY(1f, .1f));
    }

    public List<Buff<ProduceImpact>> GetBuffs()
    {
        return buffCarrier.BuffList;
    }
}