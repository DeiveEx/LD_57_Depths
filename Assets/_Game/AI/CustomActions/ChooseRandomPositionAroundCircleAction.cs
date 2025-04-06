using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Choose random point around position", story: "Choose random point around [position]", category: "Action", id: "8496bc5ebda6d7ab43c72cc3bf6f0ce9")]
public partial class ChooseRandomPositionAroundCircleAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3Int> Position;
    [SerializeReference] public BlackboardVariable<float> Range;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

