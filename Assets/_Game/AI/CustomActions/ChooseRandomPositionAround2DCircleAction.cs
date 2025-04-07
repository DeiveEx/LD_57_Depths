using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Choose random 2D point around House", story: "Choose random 2D point around House", category: "Action", id: "8496bc5ebda6d7ab43c72cc3bf6f0ce9")]
public partial class ChooseRandomPositionAround2DCircleAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<AIController> Controller;
    [SerializeReference] public BlackboardVariable<List<Vector3Int>> OutputPath;

    protected override Status OnStart()
    {
        var pointInCircle = Random.insideUnitCircle * Range;
        var intPoint = Controller.Value.HousePosition + new Vector3Int(
            Mathf.FloorToInt(pointInCircle.x), 
            0, 
            Mathf.FloorToInt(pointInCircle.y));

        var path = PathFinderService.FindPath(Controller.Value.CurrentGridPos, intPoint);

        if (path.Count == 0)
            return Status.Failure;

        OutputPath.Value = path;
        return Status.Success;
    }
}
