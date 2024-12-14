using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class UnloadCrystal : SpiritAction
{
    public override TaskStatus OnUpdate()
    {
        return Spirit.Unload() ? TaskStatus.Success : TaskStatus.Failure;
    }
}