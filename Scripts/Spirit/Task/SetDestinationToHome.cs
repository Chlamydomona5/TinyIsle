using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class SetDestinationToHome : SpiritAction
{
    public SharedVector3 InternalDestination;
    public SharedVector3 Offset;
    
    public override TaskStatus OnUpdate()
    {
        InternalDestination.SetValue(Spirit.belongUnit.model.transform.position + Offset.Value);
        Spirit.SetTolerance(0.05f);
        return TaskStatus.Success;
    }
}