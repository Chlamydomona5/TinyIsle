using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RockController : MonoBehaviour
{
    public static float RockHeight = 1f;
    private GridManager Grid => GridManager.Instance;

    [SerializeField] int maxRockCount = 5;

    [SerializeField] private List<Rock> rockPool = new();
    [SerializeField] private List<Rock> connectedRocks = new();
    [SerializeField] private List<Rock> rockPrefabs;
    [SerializeField] private GameObject rockChestModelPrefab;
    
    public List<Rock> ConnectedRocks => connectedRocks;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < maxRockCount; i++)
        {
            GenerateRock();
        }
    }

    public bool HasRock(Vector2Int coord)
    {
        return rockPool.Exists(rock => rock.GetCoveredCoords().Contains(coord));
    }

    public void UpdateConnectedRocks()
    {
        var edgeBlocks = Grid.GetEdgeBlocks();
        foreach (var rock in rockPool)
        {
            // Check if any of the rock's covered coords are around the edge blocks
            var around = Grid.GetAround4Dir(rock.GetCoveredCoords());
            if (edgeBlocks.Exists(block => around.Contains(block.coord)) && !connectedRocks.Contains(rock))
                connectedRocks.Add(rock);
        }
    }

    public bool GenerateRock()
    {
        var prefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];
        
        var coord = FindPlaceFor(prefab);
        if (coord.x == -1000) return false;

        var rock = Instantiate(prefab, transform);
        rock.Init(coord, prefab.coveredCoordsRaw, RockHeight);
        rockPool.Add(rock);
        
        return true;
    }

    private Vector2Int FindPlaceFor(Rock rock)
    {
        var tryCount = 0;
        while (tryCount < 10)
        {
            tryCount++;
            
            var target = Grid.RandomEmptyCoordInRange(3);
            // Check if the target coord is valid(No block or rock)
            bool valid = true;
            foreach (var coord in rock.coveredCoordsRaw)
            {
                if (HasRock(coord + target))
                {
                    valid = false;
                    break;
                }
            }
            // If valid, return the target coord
            if (valid) return target;
        }

        return new Vector2Int(-1000, -1000);
    }
    
    public void MineRock(Rock rock)
    {
        rockPool.Remove(rock);
        connectedRocks.Remove(rock);

        var tween = Grid.ExpandAt(rock.RealCoveredCoords);
        tween.onComplete += () =>
        {
            Grid.boxController.GenerateBox("Rock", rock.coordinate, rock.Rewards);
        };
        
        Destroy(rock.gameObject);
        GenerateRock();
    }
}