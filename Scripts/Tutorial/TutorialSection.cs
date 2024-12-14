using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public abstract class TutorialSection
{
    public abstract string ID { get; }
    public abstract IEnumerator Start();
    public abstract void End();
}

public class Section1RevealLeaves : TutorialSection
{
    public override string ID => "RevealLeaves";

    public override IEnumerator Start()
    {
        yield return null;
    }

    public override void End()
    {
        TutorialManager.Instance.touchHandler.Init();
        TutorialManager.Instance.toolButton.SetActive(true);
    }
}

public class Section2Expand : TutorialSection
{
    public override string ID => "Expand";
    public override IEnumerator Start()
    {
        var start = PropertyManager.Instance.GetExpandCrystal(false);
        var end = GridManager.Instance.RandomEmptyCoordInRange(3);
        yield return new WaitForSeconds(1f);
        TutorialManager.Instance.pointerUI.DragMode(GridManager.Instance.Coord2Pos(start), GridManager.Instance.Coord2Pos(end));
    }

    public override void End()
    {
        TutorialManager.Instance.pointerUI.TurnOff();
    }
}

public class Section3CollectCrystal : TutorialSection
{
    public override string ID => "CollectCrystal";
    public override IEnumerator Start()
    {
        var block = GridManager.Instance.crystalController.FindAllCrystalable(CrystalType.Gold);
        //Cant spawn in the center
        block.RemoveAll(x => x.x <= 2 && x.y <= 2 && x.x >= -1 && x.y >= -1);
        Vector2Int des = Vector2Int.zero;
        for (int i = 0; i < 6; i++)
        {
            des = block[Random.Range(0, block.Count)];
            GridManager.Instance.crystalController.GenerateGoldCrystalStandard(Vector3.zero, des, 5, 5, false);
        }
        yield return new WaitForSeconds(1f);
        TutorialManager.Instance.pointerUI.TapMode(GridManager.Instance.Coord2Pos(des));    
    }

    public override void End()
    {
        TutorialManager.Instance.resourcePanel.SetActive(true);
        TutorialManager.Instance.pointerUI.TurnOff();
    }
}

public class Section4LongpressAltar : TutorialSection
{
    public override string ID => "LongpressAltar";
    public override IEnumerator Start()
    {
        TutorialManager.Instance.pointerUI.LongPressMode(GridManager.Instance.Coord2Pos(Vector2Int.zero));
        yield return null;
    }

    public override void End()
    {
        TutorialManager.Instance.pointerUI.TurnOff();
    }
}

public class Section6UnlockWebUnlock : TutorialSection
{
    public override string ID => "UnlockWebUnlock";
    public override IEnumerator Start()
    {
        TutorialManager.Instance.TutorialCrystalMovable(false);
        
        GridManager.Instance.core.GetComponent<CoreUnit>().GetCoreCrystal();
        TutorialManager.Instance.unlockButton.SetActive(true);
        AppealButton(TutorialManager.Instance.unlockButton.GetComponent<RectTransform>());
        yield return new WaitForSeconds(2f);
        TutorialManager.Instance.unlockMask.SetActive(true);
        TutorialManager.Instance.newItemPanel.Open(Methods.GetLocalText("TutorialExpandCrystal"),
                Methods.GetLocalText("TutorialExpandCrystal_Desc"),
                Resources.Load<Sprite>("SpecialIcons/TutorialExpandCrystal"))
            .AddListener(() => RotateShake(TutorialManager.Instance.unlockButton.transform));
    }
    
    public override void End()
    {
        TutorialManager.Instance.TutorialCrystalMovable(true);
        
        UIManager.Instance.CloseUI();
        TutorialManager.Instance.shopButton.SetActive(true);
        AppealButton(TutorialManager.Instance.shopButton.GetComponent<RectTransform>())
            .Append(RotateShake(TutorialManager.Instance.shopButton.transform));
    }
    
    private static Sequence AppealButton(RectTransform buttonRect)
    {
        var originalPos = buttonRect.anchoredPosition;
        buttonRect.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        buttonRect.localScale = Vector3.zero;
        var sequence = DOTween.Sequence();
        sequence.Append(buttonRect.DOScale(2f, 0.25f));
        sequence.Append(buttonRect.DOScale(1f, 0.75f));
        sequence.Append(buttonRect.DOAnchorPos(originalPos, 1f).SetEase(Ease.OutCubic));
        
        return sequence;
    }

    private static Sequence RotateShake(Transform transform)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DORotate(new Vector3(0, 0, 15), 0.1f));
        sequence.Append(transform.DORotate(new Vector3(0, 0, -15), 0.1f));
        sequence.Append(transform.DORotate(new Vector3(0, 0, 0), 0.1f));
        sequence.AppendInterval(1f);
        sequence.SetLoops(5);
        
        return sequence;
    }
}