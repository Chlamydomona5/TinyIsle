using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class SpiritConditional : Conditional
{
    protected Spirit Spirit;
    
    public override void OnAwake()
    {
        base.OnAwake();
        Spirit = GetComponent<Spirit>();
    }
}