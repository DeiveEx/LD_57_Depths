using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class AStarPathFinder3D : MonoBehaviour
{
    private void Start()
    {
        NativeList<int2> path = new(Allocator.TempJob);
        
        var findPathJob = new FindPathJob()
        {
            StartPos = new int2(0, 0),
            EndPos = new int2(1, 1),
            GridSize = new int2(2, 2),
            CalculatedPath = path,
            AllowDiagonal = false,
        };
        
        var handle = findPathJob.Schedule();
        handle.Complete();

        foreach (var pos in path)
        {
            Debug.Log(pos);
        }

        path.Dispose();
    }
}
