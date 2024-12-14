using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class BackHome : SpiritAction
{
    public SharedBool IsBack;
    
    public override TaskStatus OnUpdate()
    {
        Spirit.OnSpiritBackHome(IsBack.Value);
        return TaskStatus.Success;
    }
}