using DOTSPathfinding.ThreeD;
using DOTSPathfinding.TwoD;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class AStarPathFinderTester : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== 2D");
        FindPath2D();
        Debug.Log("=== 3D");
        FindPath3D();
    }

    private void FindPath2D()
    {
        NativeList<int2> path = new(Allocator.TempJob);
        
        var findPathJob = new FindPath2DJob()
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
    
    private void FindPath3D()
    {
        NativeList<int3> path = new(Allocator.TempJob);
        
        var findPathJob = new FindPath3DJob()
        {
            StartPos = new int3(0, 0, 0),
            EndPos = new int3(1, 1, 1),
            GridSize = new int3(2, 2, 2),
            CalculatedPath = path,
            AllowDiagonal = false, //Broken if True
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
