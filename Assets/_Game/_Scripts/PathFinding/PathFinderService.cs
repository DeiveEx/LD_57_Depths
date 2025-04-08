using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PathFinderService
{
    public static List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos)
    {
        List<Vector3Int> resultPath = new();

        //We gotta dispose of NativeArrays/Lists
        using var path = new NativeList<int3>(Allocator.TempJob);
        using var weightsArray = GetChunkGridAsNodes(out var gridSize);
        
        if(!IsPositionInsideGrid(startPos, gridSize) || !IsPositionInsideGrid(endPos, gridSize))
            return resultPath;
            
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

        foreach (var pos in path)
        {
            resultPath.Add(new Vector3Int(pos.x, pos.y, pos.z));
        }
            
        resultPath.Reverse();

        return resultPath;
    }
    
    private static bool IsPositionInsideGrid(Vector3Int gridPos, Vector3Int gridSize)
    {
        return
            gridPos.x >= 0 &&
            gridPos.y >= 0 &&
            gridPos.z >= 0 &&
            gridPos.x < gridSize.x &&
            gridPos.y < gridSize.y &&
            gridPos.z < gridSize.z;
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
        
        return blockWeights;
    }
    
    private static int CalculateIndex(Vector3Int pos, Vector3Int gridSize)
    {
        return (pos.x * gridSize.y * gridSize.z) + (pos.y * gridSize.z) + pos.z;
    }

    public static void DrawPathGizmos(List<Vector3Int> path)
    {
        if(path == null || path.Count == 0)
            return;
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = Vector3.one * 0.5f + path[i];
            var b = Vector3.one * 0.5f + path[i + 1];
            
            Gizmos.DrawLine(a, b);
        }
    }
}
