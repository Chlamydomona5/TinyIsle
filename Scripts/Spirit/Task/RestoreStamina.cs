using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class RestoreStamina : SpiritAction
{
    public override TaskStatus OnUpdate()
    {
        Spirit.RestoreStamina();
        return TaskStatus.Success;
    }
}