using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MagicCactusVisual : MonoBehaviour
{
    public List<Transform> points;

    public GameObject orangeFlower;
    public GameObject blueFlower;
    public GameObject purpleFlower;
    public GameObject greenFlower;
    

    public void EvolveModel(UnitFeature_SO featureSo)
    {
        Debug.Log("EvolveModel " + featureSo.name);
        var noChildPoints = points.Where(p => p.childCount == 0).ToList();
        if (noChildPoints.Count() == 0) Debug.LogError("No empty point to place flower");
        var point = noChildPoints[Random.Range(0, noChildPoints.Count())];

        switch (featureSo.ID)
        {
            case "MagicCactus1":
                Instantiate(orangeFlower, point);
                break;
            case "MagicCactus2":
                Instantiate(blueFlower, point);
                break;
            case "MagicCactus3":
                Instantiate(greenFlower, point);
                break;
            case "MagicCactus4":
                Instantiate(purpleFlower, point);
                break;
        }
    }
}