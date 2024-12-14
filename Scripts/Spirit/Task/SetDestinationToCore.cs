using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class SetDestinationToCore : SpiritAction
{
    public override TaskStatus OnUpdate()
    {
        Spirit.SetDestination(Vector2Int.one, 1f);
        return TaskStatus.Success;
    }
}