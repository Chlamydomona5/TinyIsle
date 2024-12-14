using System;
using System.Linq;
using CarterGames.Assets.AudioManager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Reward
{
    [Serializable]
    public abstract class BoxReward
    {
        public abstract bool Reward(Box box);
    }

    [Serializable]
    public class CrystalReward : BoxReward
    {
        public CrystalType type = CrystalType.Gold;
        public int amount = 5;

        public override bool Reward(Box box)
        {
            var crystalables = GridManager.Instance.crystalController.FindAllCrystalable(type);
            if (crystalables.Count == 0) return false;

            var value = FishController.fishValue * Mathf.Sqrt(PropertyManager.Instance.AvrIncome());
            value = Mathf.Clamp(value, 10, Mathf.Infinity);

            for (int i = 0; i < amount; i++)
            {
                var place = crystalables[Random.Range(0, crystalables.Count)];
                crystalables.Remove(place);
                GridManager.Instance.crystalController.GenerateGoldCrystalSpecial(
                    GridManager.Instance.Coord2Pos(box.coordinate), place, (int)value);

                if (crystalables.Count == 0) break;
            }

            return true;
        }
    }

    [Serializable]
    public class WeatherFishReward : BoxReward
    {
        public WeatherType type;

        public override bool Reward(Box box)
        {
            var tank = TimeManager.Instance.weatherFishTank;
            if (!tank)
            {
                UIManager.Instance.TipViaID("NeedWeatherTank");
                return true;
            }
            
            if (!tank.RegisterFish(this)) return false;
            
            box.transform.FlipToCoordAnim(box.transform.position, tank.GetComponent<UnitEntity>().coordinate, 0.5f, 0).onComplete += () =>
            {
                tank.UpdateModel(box);
            };
            
            // If Success No destroy
            return false;
        }
    }
    
    [Serializable]
    public class FishSpotReward : BoxReward
    {
        public override bool Reward(Box box)
        {
            var place = GridManager.Instance.fishController.GenerateSpotRandom();
            CameraManager.Instance.Follow(box.transform, 1f);
            box.transform.FlipToCoordAnim(box.transform.position, place, 1f, 0).onComplete +=
                () =>
                {
                    AudioManager.instance.Play("CrystalDrop");
                    GameObject.Destroy(box.gameObject);
                };
            return false;
        }
    }



    [Serializable]
    public class CrystalCollectReward : BoxReward
    {
        public int amount = 5;
        public override bool Reward(Box box)
        {
            var places = GridManager.Instance.crystalController.FindAllCrystal(CrystalType.Gold).ToList();
            places.ForEach(crystal =>
            {
                GridManager.Instance.crystalController.FlipTopCrystalAt(crystal.coord, new Vector2Int(1, 1), amount);
            });
            return true;
        }
    }
 
    [Serializable]
    public class SpiritGiftReward : BoxReward
    {
        public bool isFood = true;
        public float amount = .2f;
        public override bool Reward(Box box)
        {
            var target = SpiritManager.Instance.ClosestSpirit(box.coordinate);
            if (target == default)
            {
                UIManager.Instance.TipViaID("NeedSpirit");
                return true;
            }
            box.transform.FlipToWorldPosAnim(box.transform.position, target.transform.position, 1f, 0).onComplete +=
                () =>
                {
                    if(isFood) target.BeFull();
                    target.AddHeartProgress(amount);
                    GameObject.Destroy(box.gameObject);
                };
            return false;
        }
    }
}