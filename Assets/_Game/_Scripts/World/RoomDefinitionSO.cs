using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomDefinitionSO : ScriptableObject
{
    public Vector3Int RoomSize;
    public Vector2Int MinMaxEnemies;
    public AIController[] Enemies;
    
    public bool CanSpawnAt(Vector3Int position)
    {
        BoundsInt roomBounds = new BoundsInt(position, RoomSize);
        
        //Can the room be fit into the world?
        if (!roomBounds.IsContainedBy(World.Instance.WorldBounds))
            return false;

        //Does the room overlaps with any other existing room?
        foreach (var room in World.Instance.Rooms.Values)
        {
            if (roomBounds.Intersects(room.Bounds))
                return false;
        }

        return true;
    }

    public RoomInstance SpawnAt(Vector3Int position)
    {
        var room = new RoomInstance()
        {
            WorldPosition = position,
            Definition = this,
            Bounds = new(),
        };
        
        room.Bounds.SetMinMax(position, position + RoomSize);
        
        DigRoom(room);
        SpawnEnemies(room);

        return room;
    }

    private void DigRoom(RoomInstance room)
    {
        foreach (var pos in room.Bounds.allPositionsWithin)
        {
            World.Instance.TrySetBlock(pos, BlockType.Empty);
        }
    }

    private void SpawnEnemies(RoomInstance room)
    {
        int amount = Random.Range(MinMaxEnemies.x, MinMaxEnemies.y);
        amount = Mathf.Min(amount, room.Bounds.GetPositionsCount());

        HashSet<Vector3Int> occupiedPositions = new();

        for (int i = 0; i < amount; i++)
        {
            Vector3Int position;
            
            do
            {
                position = room.Bounds.GetRandomPosition();
            }
            while(occupiedPositions.Contains(position));

            SpawnSingleEnemy(position);
            occupiedPositions.Add(position);
        }
    }

    private void SpawnSingleEnemy(Vector3Int position)
    {
        var enemyPrefab = Enemies[Random.Range(0, Enemies.Length)];
        var enemy = Instantiate(enemyPrefab, World.Instance.GetWorldCenterPosition(position), Quaternion.identity);
    }
}
