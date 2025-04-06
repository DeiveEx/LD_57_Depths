using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private ChunkRenderer _chunkRendererPrefab;
    [SerializeField] private Vector3Int _chunkSize = new (10, 10, 10);
    [SerializeField] private Vector3Int _chunkAmount = new (2, 2, 2);
    
    private Dictionary<Vector3Int, ChunkData> _chunks = new();
    private Dictionary<Vector3Int, ChunkRenderer> _chunkRenderers = new();
    private BoundsInt _worldsBounds;

    public void GenerateNewWorld()
    {
        //Delete existing chunks
        foreach (var chunkRenderer in _chunkRenderers.Values)
        {
            Destroy(chunkRenderer.gameObject);
        }
        
        _chunks.Clear();
        _chunkRenderers.Clear();
        _worldsBounds = new();

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
        
        UpdateWorldBounds();
        
        //Populate the top-most layer of the top-most chunks with air
        var topLayer = _worldsBounds.max.y - 1;

        for (int x = _worldsBounds.min.x; x < _worldsBounds.max.x; x++)
        {
            for (int z = _worldsBounds.min.z; z < _worldsBounds.max.z; z++)
            {
                var pos = new Vector3Int(x, topLayer, z);
                SetBlock(pos, BlockType.Empty);
            }
        }
    }

    public void UpdateChunks()
    {
        foreach (var chunkRenderer in _chunkRenderers.Values)
        {
            chunkRenderer.UpdateChunk();
        }
    }

    public void SetBlock(Vector3Int worldPos, BlockType type)
    {
        var chunkPos = GetChunkPosFromWorldPos(worldPos);
        
        if(!_chunks.TryGetValue(chunkPos, out var chunk))
            return;

        var localChunkPos = worldPos - chunkPos;

        if (chunk.Grid.TrySetValue(localChunkPos, type))
        {
            chunk.IsModified = true;
            _chunkRenderers[chunkPos].SetDirty();
        }
    }

    public BlockType GetBlock(Vector3Int worldPos)
    {
        var chunkPos = GetChunkPosFromWorldPos(worldPos);

        if(!_chunks.TryGetValue(chunkPos, out var chunk))
            return BlockType.Indestructible;
        
        var localChunkPos = worldPos - chunkPos;

        if (chunk.Grid.TryGetValue(localChunkPos, out var blockType))
            return blockType;

        return BlockType.Indestructible;
    }

    public Vector3Int GetGridPosFromWorldPos(Vector3 worldPos)
    {
        //If we don't clamp the position, we can use any grid to calculate
        return _chunks[Vector3Int.zero].Grid.GetGridPosFromWorldPos(worldPos);
    }
    
    public Vector3 GetWorldCenterPosition(Vector3Int worldPos)
    {
        //If we don't clamp the position, we can use any grid to calculate
        return _chunks[Vector3Int.zero].Grid.GetWorldCenterPosition(worldPos);
    }

    private Vector3Int GetChunkPosFromWorldPos(Vector3Int worldPos)
    {
        var chunkPos = new Vector3Int(
            Mathf.FloorToInt(worldPos.x / (float)_chunkSize.x) * _chunkSize.x,
            Mathf.FloorToInt(worldPos.y / (float)_chunkSize.y) * _chunkSize.y,
            Mathf.FloorToInt(worldPos.z / (float)_chunkSize.z) * _chunkSize.z
        );

        return chunkPos;
    }

    private void CreateChunk(Vector3Int chunkWorldPos)
    {
        var chunk = new ChunkData(_chunkSize, this, chunkWorldPos);
        var chunkRenderer = Instantiate(_chunkRendererPrefab, transform);
        chunkRenderer.transform.position = chunkWorldPos;
        chunkRenderer.name = $"Chunk_{chunkWorldPos}";
        
        _chunks.Add(chunk.WorldPosition, chunk);
        _chunkRenderers.Add(chunk.WorldPosition, chunkRenderer);
        
        //Temporarily fill the chunk with rock blocks
        foreach (var pos in chunk.Grid.Bounds.allPositionsWithin)
        {
            var type = BlockType.Rock;
            chunk.Grid.SetValue(pos, type);
        }
        
        chunkRenderer.Initialize(chunk);
        chunkRenderer.SetDirty();
    }

    private void UpdateWorldBounds()
    {
        Bounds a = new();
        
        foreach (var chunk in _chunks.Values)
        {
            a.Encapsulate(chunk.WorldPosition + chunk.Grid.Bounds.min);
            a.Encapsulate(chunk.WorldPosition + chunk.Grid.Bounds.max);
        }

        _worldsBounds = new(
            Mathf.FloorToInt(a.min.x),
            Mathf.FloorToInt(a.min.y),
            Mathf.FloorToInt(a.min.z),
            Mathf.FloorToInt(a.size.x),
            Mathf.FloorToInt(a.size.x),
            Mathf.FloorToInt(a.size.x)
        );
    }

    private void OnDrawGizmosSelected()
    {
        if(!Application.isPlaying)
            return;
        
        Gizmos.DrawWireCube(_worldsBounds.center, _worldsBounds.size);
    }
}
