using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class SpiritAction : Action
{
    protected Spirit Spirit;
    
    public override void OnAwake()
    {
        base.OnAwake();
        Spirit = GetComponent<Spirit>();
    }
}