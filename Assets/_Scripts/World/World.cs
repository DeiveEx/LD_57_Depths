using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private ChunkRenderer _chunkRendererPrefab;
    [SerializeField] private Vector3Int _chunkSize = new (10, 10, 10);
    [SerializeField] private Vector3Int _chunkAmount = new (2, 2, 2);
    
    public Dictionary<Vector3Int, ChunkData> Chunks = new();
    public Dictionary<Vector3Int, ChunkRenderer> ChunkRenderers = new();

    public void GenerateNewWorld()
    {
        //Delete existing chunks
        foreach (var chunkRenderer in ChunkRenderers.Values)
        {
            Destroy(chunkRenderer.gameObject);
        }
        
        Chunks.Clear();
        ChunkRenderers.Clear();

        for (int x = 0; x < _chunkAmount.x; x++)
        {
            for (int y = 0; y < _chunkAmount.y; y++)
            {
                for (int z = 0; z < _chunkAmount.z; z++)
                {
                    var worldPos = new Vector3Int(x * _chunkSize.x, y * _chunkSize.y, z * _chunkSize.z);
                    CreateChunk(worldPos);
                }
            }
        }
    }

    public void UpdateChunks()
    {
        foreach (var chunkRenderer in ChunkRenderers.Values)
        {
            chunkRenderer.UpdateChunk();
        }
    }

    public void SetBlock(Vector3Int worldPos, BlockType type)
    {
        //Get chunk position from world position
        var chunkPos = new Vector3Int(
            Mathf.FloorToInt(worldPos.x / (float)_chunkSize.x) * _chunkSize.x,
            Mathf.FloorToInt(worldPos.y / (float)_chunkSize.y) * _chunkSize.y,
            Mathf.FloorToInt(worldPos.z / (float)_chunkSize.z) * _chunkSize.z
        );
        
        if(!Chunks.TryGetValue(chunkPos, out var chunk))
            return;

        var localChunkPos = worldPos - chunkPos;

        if (chunk.Grid.TrySetValue(localChunkPos, type))
            ChunkRenderers[chunkPos].SetDirty();
    }

    private void CreateChunk(Vector3Int worldPos)
    {
        var chunk = new ChunkData(_chunkSize, this, worldPos);
        var chunkRenderer = Instantiate(_chunkRendererPrefab, transform);
        chunkRenderer.transform.position = worldPos;
        chunkRenderer.name = $"Chunk_{worldPos}";
        
        Chunks.Add(chunk.WorldPosition, chunk);
        ChunkRenderers.Add(chunk.WorldPosition, chunkRenderer);
        
        //Temporarily fill the chunk with rock blocks
        foreach (var pos in chunk.Grid.Bounds.allPositionsWithin)
        {
            var type = BlockType.Rock;
            chunk.Grid.SetValue(pos, type);
        }
        
        chunkRenderer.Initialize(chunk);
        chunkRenderer.SetDirty();
    }
}
