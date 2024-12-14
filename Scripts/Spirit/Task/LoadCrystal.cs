using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class LoadCrystal : SpiritAction
{
    public override TaskStatus OnUpdate()
    {
        return Spirit.Load() ? TaskStatus.Success : TaskStatus.Failure;
    }
}