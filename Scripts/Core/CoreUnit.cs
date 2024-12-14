using System;
using CarterGames.Assets.AudioManager;
using UnityEngine;

public class CoreUnit : MonoBehaviour
{
    public Path effect;
    private int _currentFlowerCount;
    int FlowerIndex => PropertyManager.Instance.GetCurrentCoreFlowerNumber() - 1;
    
    private void Awake()
    {
        effect = GetComponentInChildren<Path>();
    }
    
    public void UpdateFlowerCount(int count)
    {
        var diff = count - _currentFlowerCount;
        if(diff == 0) return;

        for (int i = _currentFlowerCount; i != count; i += diff / Mathf.Abs(diff))
        {
            if (diff > 0) effect.pre_UpGrade(i);
            else effect.flowerDown(i - 1);
        }
        _currentFlowerCount = count;
    }

    public bool Press(float time)
    {
        if (!PropertyManager.Instance.CanExpand) return false;

        if (!effect.isstart)
            effect.UpGrading(FlowerIndex);

        if (time > 1f)
        {
            effect.Upgrade_Sucess(FlowerIndex);
            PropertyManager.Instance.GetExpandCrystal(); 
            AudioManager.instance.Play("SelectUnit");
            TutorialManager.Instance?.endCurrentEvent.Invoke("LongpressAltar");
        }

        return time > 1f;
    }

    public void Interrupt()
    {
        if(FlowerIndex < 0) return;
        effect.Upgrade_Fail(FlowerIndex);
    }

    public void GetCoreCrystal()
    {
        effect.TutorialFlash();
    }
}