using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class TryGetAvailableStackPos : SpiritAction
{
    public override TaskStatus OnUpdate()
    {
        if (SpiritManager.Instance.TryGetAvailableStack(out var newDes))
        {
            Debug.Log("New destination: " + newDes);
            Spirit.SetDestination(newDes);
            return TaskStatus.Success;
        }
        else return TaskStatus.Failure;
    }
}
