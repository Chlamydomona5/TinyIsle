using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class RotateTowardHome : SpiritAction
{
    public override TaskStatus OnUpdate()
    {
        var home = Spirit.belongUnit;
        transform.rotation = Quaternion.LookRotation(Methods.YtoZero(home.model.transform.position - transform.position), Vector3.up);
        return TaskStatus.Success;
    }
}
