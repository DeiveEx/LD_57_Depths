using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class AStarPathFinder
{
    public static List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos)
    {
        List<Vector3Int> resultPath = new();

        //We gotta dispose of NativeArrays/Lists. We can link multiple usings statements like this
        using (var path = new NativeList<int3>(Allocator.TempJob))
        using (var weightsArray = GetChunkGridAsNodes(out var gridSize))
        {
            var findPathJob = new PathFinding3DByBlockWeightJob()
            {
                StartPos = new int3(startPos.x, startPos.y, startPos.z),
                EndPos = new int3(endPos.x, endPos.y, endPos.z),
                GridSize = new int3(gridSize.x, gridSize.y, gridSize.z),
                CalculatedPath = path,
                WeightsArray = weightsArray,
            };
        
            var handle = findPathJob.Schedule();
            handle.Complete();

            Debug.Log($"Path: {path.Length}");
        
            foreach (var pos in path)
            {
                resultPath.Add(new Vector3Int(pos.x, pos.y, pos.z));
            }
        }

        resultPath.Reverse();
        return resultPath;
    }
    
    private static NativeArray<int> GetChunkGridAsNodes(out Vector3Int gridSize)
    {
        //Lets use the first chunk for now
        var chunk = World.Instance.Chunks[Vector3Int.zero];
        gridSize = chunk.Grid.Bounds.size;

        NativeArray<int> blockWeights = new(gridSize.x * gridSize.y * gridSize.z, Allocator.TempJob);
        
        foreach (var pos in chunk.Grid.Bounds.allPositionsWithin)
        {
            int index = CalculateIndex(pos, gridSize);
            blockWeights[index] = chunk.Grid.GetValue(pos).Data.PathFindTravelWeight;
        }
        
        //Add some walls
        Vector3Int[] wallPositions = new[]
        {
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 1, 1),
            new Vector3Int(0, 1, 1),
        };

        foreach (var pos in wallPositions)
        {
            int index = CalculateIndex(pos, gridSize);
            blockWeights[index] = 9999;
        }

        return blockWeights;
    }
    
    private static int CalculateIndex(Vector3Int pos, Vector3Int gridSize)
    {
        return (pos.x * gridSize.y * gridSize.z) + (pos.y * gridSize.z) + pos.z;
    }

    public static void DrawPathGizmos(List<Vector3Int> path, Color? color = null)
    {
        if(path == null || path.Count == 0)
            return;
        
        color ??= Color.red;
        Gizmos.color = color.Value;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];
            
            Gizmos.DrawLine(a, b);
        }
    }
}
