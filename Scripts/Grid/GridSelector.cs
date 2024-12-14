using System;
using System.Collections.Generic;
using System.Linq;
using Unit;
using UnityEngine;
using UnityEngine.Serialization;

public class GridSelector : MonoBehaviour
{
    [SerializeField] private MeshRenderer rangePrefab;
    [SerializeField] private MeshRenderer coveredPrefab;
    
    private List<MeshRenderer> _currentCovereds = new();
    private List<MeshRenderer> _currentRanges = new();

    public void AdjustWithUnit(Unit_SO unit)
    {
        Clear();

        //Add new slots
        var coveredCoords = unit.coveredCoords;
        foreach (var coveredCoord in coveredCoords)
        {
            var slot = Instantiate(coveredPrefab, transform);
            slot.transform.localPosition = new Vector3(coveredCoord.x, 0.1f, coveredCoord.y);
            _currentCovereds.Add(slot);
        }
        
        //Add range
        if(unit.features.FirstOrDefault(x => x.effect is RangeEffect) is { } rangeFeature)
        {
            var coords = ((RangeEffect)rangeFeature.effect).GetTargetCoords(unit);
            foreach (var coord in coords)
            {
                if(unit.coveredCoords.Exists(x => x == coord)) continue;
                
                var slot = Instantiate(rangePrefab, transform);
                slot.transform.localPosition = new Vector3(coord.x, 0.1f, coord.y);
                _currentRanges.Add(slot);
            }
        }
    }

    private void Clear()
    {
        //Clear previous slots
        foreach (var slot in _currentCovereds.Concat(_currentRanges))
        {
            Destroy(slot.gameObject);
        }
        _currentCovereds.Clear();
        _currentRanges.Clear();
    }

    public void AdjustToNormal()
    {
        Clear();
        
        var slot = Instantiate(coveredPrefab, transform);
        slot.transform.localPosition = new Vector3(0, 0.1f, 0);
        _currentCovereds.Add(slot);
    }
    
    public void ChangeMode(bool canBuild)
    {
        foreach (var slot in _currentCovereds.Concat(_currentRanges))
        {
            slot.material.SetColor("_Color",canBuild ? Color.white : Color.red);
        }
    }
}