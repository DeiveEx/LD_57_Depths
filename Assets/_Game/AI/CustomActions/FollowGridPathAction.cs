using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Follow Grid Path", story: "Follow Grid [Path]", category: "Action", id: "c0ca5152156d4cd39896c94d27912447")]
public partial class FollowGridPathAction : Action
{
    [SerializeReference] public BlackboardVariable<List<Vector3Int>> Path;
    [SerializeReference] public BlackboardVariable<AIController> Controller;

    protected override Status OnStart()
    {
        if(Path.Value == null || Path.Value.Count == 0)
            return Status.Failure;
        
        Controller.Value.FollowGridPath(Path.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Controller.Value.IsFollowingPath)
            return Status.Running;
        
        return Status.Success;
    }
}

