using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class ConsumeStamina : SpiritAction
{
    public override TaskStatus OnUpdate()
    {
        Spirit.ConsumeStamina();
        return TaskStatus.Success;
    }
}