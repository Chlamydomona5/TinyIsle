using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class IsDestinationStackAvailable : SpiritConditional
{
    public override TaskStatus OnUpdate()
    {
        return SpiritManager.Instance.IsCoordStackAvailable(Spirit.Destination)
            ? TaskStatus.Success : TaskStatus.Failure;
    }
}
