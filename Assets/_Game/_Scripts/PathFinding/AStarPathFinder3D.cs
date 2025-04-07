using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class AStarPathFinder3D : MonoBehaviour
{
    private List<Vector3> _path = new();
    private Vector3Int _gridSize = new(2, 2, 2);
    
    private void Start()
    {
        NativeList<int3> path = new(Allocator.TempJob);
        
        var findPathJob = new FindPath3DJob()
        {
            StartPos = new int3(0, 0, 0),
            EndPos = new int3(1, 1, 1),
            GridSize = new int3(_gridSize.x, _gridSize.y, _gridSize.z),
            CalculatedPath = path,
            AllowDiagonal = true,
        };
        
        var handle = findPathJob.Schedule();
        handle.Complete();

        foreach (var pos in path)
        {
            Debug.Log(pos);
            _path.Add(new Vector3(pos.x, pos.y, pos.z));
        }

        path.Dispose();
    }

    private void OnDrawGizmos()
    {
        if(_path == null)
            return;
        
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int z = 0; z < _gridSize.z; z++)
                {
                    Gizmos.DrawWireCube(new Vector3(x, y, z), Vector3.one);
                }
            }
        }
        
        Gizmos.color = Color.red;

        for (int i = 0; i < _path.Count - 1; i++)
        {
            var a = _path[i];
            var b = _path[i + 1];
            
            Gizmos.DrawLine(a, b);
        }

        Gizmos.DrawSphere(Vector3.zero, 0.1f);
    }
}
