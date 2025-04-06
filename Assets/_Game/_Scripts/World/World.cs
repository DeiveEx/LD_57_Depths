using System.Collections.Generic;
using UnityEngine;

public class World : MonoSingleton<World>
{
    [SerializeField] private ChunkRenderer _chunkRendererPrefab;
    [SerializeField] private Vector3Int _chunkSize = new (10, 10, 10);
    [SerializeField] private Vector3Int _chunkAmount = new (2, 2, 2);
    [SerializeField] private Vector2Int _minMaxRoomAmount = new (1, 5);
    [SerializeField] private RoomDefinitionSO[] _roomDefinitions;
    
    private BoundsInt _worldBounds;
    private Dictionary<Vector3Int, ChunkData> _chunks = new();
    private Dictionary<Vector3Int, ChunkRenderer> _chunkRenderers = new();
    private Dictionary<Vector3Int, RoomInstance> _rooms = new();

    public BoundsInt WorldBounds => _worldBounds;
    public Dictionary<Vector3Int, RoomInstance> Rooms => _rooms;

    public void GenerateNewWorld()
    {
        //Delete existing chunks
        foreach (var chunkRenderer in _chunkRenderers.Values)
        {
            Destroy(chunkRenderer.gameObject);
        }
        
        _worldBounds = new();
        
        _chunks.Clear();
        _chunkRenderers.Clear();
        _rooms.Clear();

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
        
        ConfigureBorders();
        SpawnRooms();
    }

    public void UpdateChunks()
    {
        foreach (var chunkRenderer in _chunkRenderers.Values)
        {
            chunkRenderer.UpdateChunk();
        }
    }

    public bool TrySetBlock(Vector3Int worldPos, BlockType type)
    {
        var chunkPos = GetChunkPosFromWorldPos(worldPos);
        
        if(!_chunks.TryGetValue(chunkPos, out var chunk))
            return false;

        var localChunkPos = worldPos - chunkPos;

        if (!chunk.Grid.TrySetValue(localChunkPos, type))
            return false;
            
        chunk.IsModified = true;
        _chunkRenderers[chunkPos].SetDirty();
        return true;
    }

    public BlockType GetBlock(Vector3Int worldPos)
    {
        var chunkPos = GetChunkPosFromWorldPos(worldPos);

        if(!_chunks.TryGetValue(chunkPos, out var chunk))
            return BlockType.None;
        
        var localChunkPos = worldPos - chunkPos;

        if (!chunk.Grid.TryGetValue(localChunkPos, out var blockType))
            return BlockType.None;

        return blockType;
    }

    public Vector3Int GetGridPosFromWorldPos(Vector3 worldPos)
    {
        //If we don't clamp the position, we can use any grid to calculate
        return _chunks[Vector3Int.zero].Grid.GetGridPosFromWorldPos(worldPos, false);
    }
    
    public Vector3 GetWorldCenterPosition(Vector3Int worldPos)
    {
        //If we don't clamp the position, we can use any grid to calculate
        return _chunks[Vector3Int.zero].Grid.GetWorldCenterPosition(worldPos, false);
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
        var chunk = new ChunkData(_chunkSize, chunkWorldPos);
        var chunkRenderer = Instantiate(_chunkRendererPrefab, transform);
        chunkRenderer.transform.position = chunkWorldPos;
        chunkRenderer.name = $"Chunk_{chunkWorldPos}";
        
        _chunks.Add(chunk.WorldPosition, chunk);
        _chunkRenderers.Add(chunk.WorldPosition, chunkRenderer);
        
        //Fill the chunk with rock blocks
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
        Bounds newBounds = new();
        
        foreach (var chunk in _chunks.Values)
        {
            newBounds.Encapsulate(chunk.WorldPosition + chunk.Grid.Bounds.min);
            newBounds.Encapsulate(chunk.WorldPosition + chunk.Grid.Bounds.max);
        }

        _worldBounds = new(
            Mathf.FloorToInt(newBounds.min.x),
            Mathf.FloorToInt(newBounds.min.y),
            Mathf.FloorToInt(newBounds.min.z),
            Mathf.FloorToInt(newBounds.size.x),
            Mathf.FloorToInt(newBounds.size.y),
            Mathf.FloorToInt(newBounds.size.z)
        );
    }

    private void ConfigureBorders()
    {
        //Populate the top-most layer of the top-most chunks with air and the bottom-most layer with indestructible blocks
        var topLayer = _worldBounds.max.y - 1;
        var bottomLayer = 0;

        for (int x = _worldBounds.min.x; x < _worldBounds.max.x; x++)
        {
            for (int z = _worldBounds.min.z; z < _worldBounds.max.z; z++)
            {
                TrySetBlock(new Vector3Int(x, topLayer, z), BlockType.Empty);
                TrySetBlock(new Vector3Int(x, bottomLayer, z), BlockType.Indestructible);
            }
        }
    }

    private void SpawnRooms()
    {
        int roomCount = Random.Range(_minMaxRoomAmount.x, _minMaxRoomAmount.y);

        for (int i = 0; i < roomCount; i++)
        {
            int tries = 100;

            var position = new Vector3Int();
            var roomDefinition = _roomDefinitions[Random.Range(0, _roomDefinitions.Length)];

            do
            {
                position = new Vector3Int(
                    Random.Range(_worldBounds.min.x, _worldBounds.max.x),
                    Random.Range(_worldBounds.min.y, _worldBounds.max.y),
                    Random.Range(_worldBounds.min.z, _worldBounds.max.z)
                );

                tries--;
            }
            while (!roomDefinition.CanSpawnAt(position) && tries > 0);
            
            if (tries <= 0)
            {
                Debug.LogWarning($"Failed to spawn room {roomDefinition.name} at {position}");
                continue;
            }

            var roomInstance = roomDefinition.SpawnAt(position);
            _rooms.Add(position, roomInstance);
        }

        Debug.Log($"Spawned {_rooms.Count}/{roomCount} rooms");
    }
    
    private void OnDrawGizmosSelected()
    {
        if(!Application.isPlaying)
            return;
        
        Gizmos.DrawWireCube(_worldBounds.center, _worldBounds.size);
    }
}
