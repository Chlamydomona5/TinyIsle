using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Spirit")]
public class MoveToward : SpiritAction
{
    public SharedBool InternalDestination;
    public SharedVector3 Destination;
    
    private Rigidbody _rigidbody;
    
    public override void OnAwake()
    {
        base.OnAwake();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override TaskStatus OnUpdate()
    {
        var speed = Spirit.CurrentParam.speed;
        
        var pos = InternalDestination.Value ?  Destination.Value : GridManager.Instance.Coord2Pos(Spirit.Destination);
        
        var distance = Methods.YtoZero(pos - transform.position);
        var vector = distance.normalized * speed;
        //Move toward
        transform.rotation = Quaternion.LookRotation(distance, Vector3.up);
        if(_rigidbody.velocity.magnitude < speed)
            _rigidbody.AddForce(vector * 0.1f, ForceMode.VelocityChange); 
        //If arrive, return success
        if (distance.magnitude < Spirit.MoveTolerance)
        {
            _rigidbody.velocity = Vector3.zero;
            return TaskStatus.Success;
        }
        else return TaskStatus.Failure;
    }
    
}
