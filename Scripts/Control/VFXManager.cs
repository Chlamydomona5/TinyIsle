using System.Collections.Generic;
using Core;
using DamageNumbersPro;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class VFXManager : Singleton<VFXManager>
{
    [SerializeField] private DamageNumberMesh goldNumberPrefab;

    [SerializeField] private TimerVFX timerPrefab;
    
    [OdinSerialize] private Dictionary<string,GameObject> vfxPrefabs = new Dictionary<string, GameObject>();

    [OdinSerialize] private Dictionary<Rarity, GameObject> buildVFXPrefabs = new();
    
    public void ProduceHint(Vector3 pos, float amount)
    {
        if (amount > 0)
        {
            var number = Instantiate(goldNumberPrefab, pos, Quaternion.identity,
                transform);
            number.number = amount;
        }
    }
    
    public void ProduceTimer(Transform follow, UnityAction onTimeUp, float timeLimit, float countTime, bool willDestroy = true, bool willLoop = false)
    {
        var timer = Instantiate(timerPrefab, follow.position, Quaternion.identity, follow);
        timer.SetTimeTo(timeLimit, countTime, willDestroy, willLoop);
        timer.OnTimeUp.AddListener(onTimeUp);
    }
    
    public GameObject PlayVFX(string vfxName, Vector3 pos)
    {
        if (vfxPrefabs.TryGetValue(vfxName, out var prefab))
        {
            var vfx = Instantiate(prefab, pos, Quaternion.identity);
            Destroy(vfx, 10f);
            return vfx;
        }
        
        return null;
    }
    
    public GameObject PermanentVFX(string vfxName, Transform trans, Vector3 localPos)
    {
        if (vfxPrefabs.TryGetValue(vfxName, out var prefab))
        {
            var vfx = Instantiate(prefab, Vector3.zero, Quaternion.identity, trans);
            vfx.transform.localPosition = localPos;
            return vfx;
        }
        return null;
    }

    public void ProduceBuildEffect(UnitEntity unitEntity)
    {
        //TODO: Add build effect
    }
}