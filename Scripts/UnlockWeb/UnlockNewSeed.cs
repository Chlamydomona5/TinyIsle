
using System.Collections.Generic;
using UnityEngine;

public class UnlockNewSeed : UnlockEffect
{
    public override Sprite Icon => Resources.Load<Sprite>("UnlockIcons/TheSeed");
    public override bool IsAbleToEffect()
    {
        return GridManager.Instance.FindPlaceForEntity(
            new List<Vector2Int>() { Vector2Int.zero }).x != -1000;
    }

    public override void Effect()
    {
        GridManager.Instance.boxController.GenerateBox("TheSeed", GridManager.Instance.FindPlaceForEntity(
            new List<Vector2Int>() { Vector2Int.zero })
        );
        TutorialManager.Instance.newItemPanel.Open(Methods.GetLocalText("TheSeed"),
            Methods.GetLocalText("TheSeed_Desc"), Resources.Load<Sprite>("UnlockIcons/TheSeed"));
    }
}
