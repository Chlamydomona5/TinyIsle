using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class IsCrystalStackFullOrClear : SpiritConditional
{
    public override TaskStatus OnUpdate()
    {
        var full = Spirit.CrystalStack?.Count >= Spirit.CurrentParam.maxLoad;
        SpiritManager.Instance.TryGetAvailableStack(out var coord);
        var clear = coord == Vector2Int.zero;
        return full || clear ? TaskStatus.Success : TaskStatus.Failure;
    }
}
