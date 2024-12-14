using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Core;

public class PortageCreature : SerializedMonoBehaviour
{
    public static float CreatureHeight = 2f;

    [ShowInInspector, ReadOnly] private Vector2Int _destination;
    [ShowInInspector, ReadOnly] private float _moveTolerance = 0.1f;
    public Vector2Int Destination => _destination;
    public float MoveTolerance => _moveTolerance;

    public PortageUnitEntity belongUnit;
    public GameObject model;
    public Stack<Crystal> CrystalStack = new Stack<Crystal>();
    public bool IsCarryCrystal => CrystalStack.Count > 0;
    
    public BuffCarrier<PortageImpact> buffCarrier;

    public PortageImpact CurrentParam
    {
        get
        {
            var origin = new PortageImpact();
            origin.stamina = belongUnit.portageUnitSo.stamina;
            origin.maxLoad = belongUnit.portageUnitSo.maxLoad;
            origin.speed = belongUnit.portageUnitSo.speed;
            
            foreach (var buff in buffCarrier.BuffList)
            {
                origin.stamina += buff.Impact.stamina;
                origin.maxLoad += buff.Impact.maxLoad;
                origin.speed += buff.Impact.speed;
            }
            
            return origin;
        }
    }

    public Transform handPos;
    public Animator animator;
    
    [ShowInInspector, ReadOnly] private int _currentStamina;
    public int CurrentStamina => _currentStamina;
    private Tween _crystalHandleTween;
    
    protected Collider Collider;
    
    public void Init(PortageUnitEntity home)
    {
        animator = GetComponentInChildren<Animator>();
        Collider = GetComponentInChildren<Collider>();

        belongUnit = home;

        transform.position = new Vector3(home.transform.position.x, CreatureHeight, home.transform.position.z);
        //Change all children layer
        gameObject.layer = LayerMask.NameToLayer("Creature");
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Creature");
        }
        
        _currentStamina = CurrentParam.stamina;
    }

    public bool Load()
    {
        if(_crystalHandleTween != null && _crystalHandleTween.IsActive()) return false;
        if(!GridManager.Instance.crystalController.HasCrystal(_destination)) return true;
        
        var crystal = GridManager.Instance.crystalController.PickCrystalAt(_destination);
        CrystalStack.Push(crystal);
        crystal.TurnOffTrail();
        _crystalHandleTween = crystal.transform.DOMove(handPos.position, 0.2f).SetEase(Ease.InQuad).OnComplete((() =>
        {
            crystal.transform.SetParent(handPos, false);
            crystal.transform.localPosition = Vector3.up * (0.25f * CrystalStack.Count);
        }));

        return false;
    }

    public bool Unload()
    {
        if(_crystalHandleTween != null && _crystalHandleTween.IsActive()) return false;
        if(CrystalStack.Count == 0) return true;
        
        var crystal = CrystalStack.Pop();
        crystal.transform.SetParent(null);
        _crystalHandleTween = crystal.transform.DOMoveY(0f, 0.2f).SetEase(Ease.OutExpo).OnComplete((() =>
        {
            crystal.transform.SetParent(null, true);
            GridManager.Instance.crystalController.PushCrystalTo(_destination, crystal);
        }));
        
        return false;
    }
    
    public void SetDestination(Vector2Int coord, float tolerance = 0.1f)
    {
        _destination = coord;
        _moveTolerance = tolerance;
    }
    
    public void SetTolerance(float tolerance)
    {
        _moveTolerance = tolerance;
    }
    
    public void ConsumeStamina()
    {
        _currentStamina--;
    }
    
    public void RestoreStamina()
    {
        _currentStamina = CurrentParam.stamina;
    }
}