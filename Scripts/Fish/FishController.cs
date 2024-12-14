using System.Collections;
using System.Collections.Generic;
using CarterGames.Assets.AudioManager;
using DamageNumbersPro;
using DG.Tweening;
using Reward;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishController : SerializedMonoBehaviour
{
    [Title("Parameters")] 
    public static int spotLimitNatural = 2;
    public static int spotLimitGenerate = 5;
    public static float fishSpawnBaseInterval = 15f;
    public static float fishSpawnNoise = 5f;
    public static float fishMaxRange = 15f;
    
    public static float fishBitePeriod = 0.8f;
    
    public static float fishValue = 2f;
    public static float fishSpotDisappearTime = 30f;
    
    public float FishSpawnInterval
    {
        get
        {
            var result = fishSpawnBaseInterval;
            result += Random.Range(-fishSpawnNoise, fishSpawnNoise);
            result = Mathf.Max(0.1f, result);
            return result;
        }
    }
    
    [Title("References")]
    
    [SerializeField] private FishSpotEntity spotEntityPrefab;
    [SerializeField, ReadOnly] private List<FishSpotEntity> spots = new();

    [SerializeField] private FishSpotSet _spotSet = new();

    [SerializeField] private Transform hook;
    
    [SerializeField] private TouchHandler touchHandler;
    [SerializeField] private NewItemPanel newItemPanel;
    
    [SerializeField, ReadOnly] private bool onBite;
    [SerializeField, ReadOnly] private FishSpotEntity currentSpotEntity;
    [SerializeField, ReadOnly] private List<Fish_SO> hadFishList = new();

    [SerializeField] private DamageNumber biteSign;
    
    private bool _rightTap;
    private Coroutine _fishCoroutine;
    private Coroutine _spotGenerationCoroutine;
    
    private Fish_SO _currentFish;
    
    public void EnableFish(bool enable)
    {
        if (enable)
        {
            _spotGenerationCoroutine = StartCoroutine(SpotGeneration());
        }
        else
        {
            if(_spotGenerationCoroutine != null) StopCoroutine(_spotGenerationCoroutine);
            _spotGenerationCoroutine = null;
        }
    }

    private IEnumerator SpotGeneration()
    {
        //In Real game, wait for tutorial
        if(!TestManager.Instance.onTest)
            yield return new WaitForSeconds(30f);
        
        while (true)
        {
            if (spots.Count < spotLimitNatural)
            {
                GenerateSpotRandom();
            }

            yield return new WaitForSeconds(FishSpawnInterval);
        }
    }

    public Vector2Int GenerateSpot(FishSpot_SO spotSo)
    {
        if (spots.Count >= spotLimitGenerate) return new Vector2Int(-1000, -1000);
        
        var coord = GridManager.Instance.RandomEmptyCoordInRange(1);
        var pos = GridManager.Instance.Coord2Pos(coord);
        
        var spot = Instantiate(spotEntityPrefab, pos, Quaternion.identity, transform);
        spot.Init(spotSo, this);
        spots.Add(spot);

        return coord;
    }

    public Vector2Int GenerateSpotRandom()
    {
        return GenerateSpot(_spotSet.GetSpot());
    }

    public void RemoveSpot(FishSpotEntity spotEntity)
    {
        spots.Remove(spotEntity);
    }

    public void StartFishAt(FishSpotEntity spotEntity)
    {
        currentSpotEntity = spotEntity;
        _currentFish = spotEntity.GetFish();
        
        var position = spotEntity.transform.position;
        CameraManager.Instance.MoveTo(new Vector3(position.x, 0, position.z));
        CameraManager.Instance.ForbidTouch(true);
        
        hook.gameObject.SetActive(true);
        hook.position = position + Vector3.up * 20f;
        hook.DOMoveY(0f, 1f).SetEase(Ease.OutCubic).OnComplete((() =>
        {
            _fishCoroutine = StartCoroutine(Fishing());
        }));
    }

    private IEnumerator Fishing()
    {
        currentSpotEntity?.castParticle?.Play();

        touchHandler.touchState = TouchState.Fish;

        var intervalTimer = -1f;
        var biteTimer = -1f;
        var fakeBiteTimes = Random.Range(_currentFish.fishFakeBaseBiteTimes.x, _currentFish.fishFakeBaseBiteTimes.y);
        var biteCount = 0;
        
        onBite = false;
        _rightTap = false;

        while (true)
        {
            //If the tap is on bite
            if (_rightTap)
            {
                onBite = false;
                _rightTap = false;
                break;
            }

            //If time reachs interval, bite
            if (intervalTimer < 0f)
            {
                intervalTimer = _currentFish.GetInterval();

                onBite = biteCount >= fakeBiteTimes;
                biteCount++;
                if (onBite) biteTimer = fishBitePeriod;
                BiteMotion(onBite);
            }

            //If real bites, start to count period
            if (onBite)
            {
                biteTimer -= Time.deltaTime;
                onBite = biteTimer >= 0f;
                //If the bite is real and player missed, end fishing
                if(!onBite) EndFish();
            }
            //Interval counts when not bited
            else
            {
                intervalTimer -= Time.deltaTime;
            }

            yield return null;
        }

        EndFish();
        AudioManager.instance.Play("SelectUnit");

        //Reward
        GenerateFish(_currentFish, currentSpotEntity.transform.position + Vector3.up);
    }

    public void GenerateFish(Fish_SO fish, Vector3 pos)
    {
        var coord = GridManager.Instance.FindPlaceForEntity(fish.coveredCoords, GroundType.Low);
        if (coord.x == -1000) coord = GridManager.Instance.FindPlaceForEntity(fish.coveredCoords, GroundType.Normal);
        if (coord.x == -1000) coord = GridManager.Instance.FindPlaceForEntity(fish.coveredCoords, GroundType.High);
        if (coord.x == -1000) return;

        var entity = GridManager.Instance.boxController.GenerateFish(fish, pos, coord);
        CameraManager.Instance.Follow(entity.transform, 0.8f);
        
        if(hadFishList.Contains(fish)) return;
        hadFishList.Add(fish);
        newItemPanel.Open(fish.LocalizeName, fish.Description, fish.Icon);
        
        AchievementManager.Instance.AssignProgress(AchievementType.FishTypeCount, hadFishList.Count);
    }

    private void EndFish()
    {
        if(_fishCoroutine != null) StopCoroutine(_fishCoroutine);
        _fishCoroutine = null;
        
        CameraManager.Instance.ForbidTouch(false);
        touchHandler.touchState = TouchState.Camera;
        hook.gameObject.SetActive(false);
        if(currentSpotEntity)
            Destroy(currentSpotEntity.gameObject);
    }

    private void BiteMotion(bool real)
    {
        if (real)
        {
            AudioManager.instance.Play("Bite_Heavy");
            currentSpotEntity?.heavyParticle.Play();
            biteSign.Spawn(hook.position);
            hook.DOMoveY(hook.position.y - 1f, fishBitePeriod / 2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutCubic);
        }
        else
        {
            AudioManager.instance.Play("Bite_Light");
            currentSpotEntity?.lightParticle.Play();
            hook.DOMoveY(hook.position.y - 0.2f, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutCubic);
        }
    }

    public void OnTap()
    {
        if (onBite) _rightTap = true;
        else
        {
            AudioManager.instance.Play("FishFail");
            EndFish();
        }
    }
    

}