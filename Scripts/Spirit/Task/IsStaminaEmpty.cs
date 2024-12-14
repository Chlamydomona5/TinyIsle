using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class IsStaminaEmpty : SpiritConditional
{
    public override TaskStatus OnUpdate()
    {
        return Spirit.CurrentStamina <= 0 ? TaskStatus.Success : TaskStatus.Failure;
    }
}
