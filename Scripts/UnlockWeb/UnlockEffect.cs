
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class UnlockEffect
{
    public virtual int MaxLevel => 1;
    public abstract Sprite Icon { get; }
    public virtual string ForbiddenID => "";

    public virtual string ExtraDescription(int currentLevel) => "";
    public abstract bool IsAbleToEffect();
    public abstract void Effect();
    public virtual void Upgrade(){}
}