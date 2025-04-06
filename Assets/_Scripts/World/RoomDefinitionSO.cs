using UnityEngine;

[CreateAssetMenu]
public class RoomDefinitionSO : ScriptableObject
{
    public Vector3Int RoomSize;
    
    public bool CanSpawnAt(Vector3Int position)
    {
        //Can the room be fir into the world?
        if (!World.Instance.WorldBounds.Contains(position) ||
            !World.Instance.WorldBounds.Contains(position + RoomSize))
            return false;

        //Does the room overlaps with any other existing room?
        foreach (var room in World.Instance.Rooms.Values)
        {
            if (room.RoomBounds.Contains(position) ||
                room.RoomBounds.Contains(position + RoomSize))
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
            RoomBounds = new(),
        };
        
        instance.RoomBounds.SetMinMax(position, position + RoomSize);

        foreach (var pos in instance.RoomBounds.allPositionsWithin)
        {
            World.Instance.SetBlock(pos, BlockType.Empty);
        }

        return instance;
    }
}
