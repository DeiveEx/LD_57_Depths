using UnityEngine;

[CreateAssetMenu]
public class RoomDefinitionSO : ScriptableObject
{
    public Vector3Int RoomSize;
    
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
        var instance = new RoomInstance()
        {
            WorldPosition = position,
            Definition = this,
            Bounds = new(),
        };
        
        instance.Bounds.SetMinMax(position, position + RoomSize);

        foreach (var pos in instance.Bounds.allPositionsWithin)
        {
            World.Instance.TrySetBlock(pos, BlockType.Empty);
        }

        return instance;
    }
}
