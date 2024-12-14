using UnityEngine;


public class UnlockAchievement : UnlockEffect
{
    public override Sprite Icon => Resources.Load<Sprite>("UnlockIcons/Achievement");
    public override bool IsAbleToEffect()
    {
        return true;
    }

    public override void Effect()
    {
        
    }
}
