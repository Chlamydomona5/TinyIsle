using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestManager : Singleton<TestManager>
{
    public bool onTest = true;
    
    public bool freeEvolve = false;
    public bool freeUnlock = false;
    public bool allInShop = false;

    [SerializeField] private float startY = 300;

    private void Start()
    {
        if(onTest)
        {
            PropertyManager.Instance.AddToTestLimit();
            PropertyManager.Instance.AddCurrentGoldLimit(5);
            PropertyManager.Instance.AddProperty(100000);
        }
    }

    private void OnGUI()
    {
        if (!onTest) return;
        
        
        //Bigger Toggle
        GUI.skin.toggle.fontSize = 40;
        
        freeEvolve = GUI.Toggle(new Rect(10, startY, 200, 50), freeEvolve, "FreeEvolve");
        freeUnlock = GUI.Toggle(new Rect(10, startY + 50, 200, 50), freeUnlock, "FreeUnlock");
        allInShop = GUI.Toggle(new Rect(10, startY + 100, 200, 50), allInShop, "AllInShop");
        
        //Bigger Button
        GUI.skin.button.fontSize = 40;

        if (GUI.Button(new Rect(10, startY + 200, 200, 50), "AllLow"))
        {
            var blocks = GridManager.Instance.GetAllBlocks();
            foreach (var ground in blocks)
            {
                //Find a 2x2 blocks
                if(GridManager.Instance.CanBeSpecialSquare(ground, 2) && GridManager.Instance.GetGroundType(ground) != GroundType.High)
                {
                    GridManager.Instance.ChangeGroundType2X2(ground, GroundType.Low);
                }
            }
            GridManager.Instance.UpdateVisual();
        }
        
        //Add Gold
        if (GUI.Button(new Rect(10, startY + 250, 200, 50), "AddGold"))
        {
            PropertyManager.Instance.AddProperty(100000, "", false);
        }
        
        //Generate All Fish
        if (GUI.Button(new Rect(10, startY + 350, 200, 50), "GenerateFish"))
        {
            var list = Resources.LoadAll<Fish_SO>("Fish");
            foreach (var fish in list)
            {
                GridManager.Instance.fishController.GenerateFish(fish, Vector3.zero);
            }
        }
        
        //Time Stage Next
        if(GUI.Button(new Rect(10, startY + 400, 200, 50), "TimeStageNext"))
        {
            TimeManager.Instance.SetTimeStageToNext();
        }
        //Timer
        GUI.skin.textField.fontSize = 40;
        GUI.TextField(new Rect(10, startY + 450, 200, 50), TimeManager.Instance.DayTimer.ToString() + "/" + TimeManager.Instance.CurrentTime);
        
        //Generate The Seed
        if(GUI.Button(new Rect(10, startY + 500, 200, 50), "GenerateSeed"))
        {
            GridManager.Instance.boxController.GenerateBox("TheSeed", GridManager.Instance.FindPlaceForEntity(
                new List<Vector2Int>() { Vector2Int.zero })
            );
        }
        
        // Save current
        if(GUI.Button(new Rect(10, startY + 550, 200, 50), "SaveCurrent"))
        {
            GameSaveManager.Instance.SaveCurrent("Test");
        }
        
        // Load
        if(GUI.Button(new Rect(10, startY + 600, 200, 50), "Load"))
        {
            GameSaveManager.Instance.Load("Test");
        }
        
        // Generate Fish Spot
        if(GUI.Button(new Rect(10, startY + 650, 200, 50), "GenerateFishSpot"))
        {
                GridManager.Instance.fishController.GenerateSpotRandom();
        }
    }
}