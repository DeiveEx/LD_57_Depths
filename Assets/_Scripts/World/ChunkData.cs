using Ignix.GenericGridSystem;
using UnityEngine;

public class ChunkData
{
	public GenericGrid<BlockType> Grid;
	public Vector3Int ChunkSize;
	public World WorldReference;
	public Vector3Int WorldPosition;

	/// <summary>
	/// This is true whenever the chunk suffered any modification. Chunks that were not modified can be regenerated them from scratch.
	/// </summary>
	public bool IsModified = false;

	public ChunkData(Vector3Int chunkSize, World worldReference, Vector3Int worldPosition)
	{
		ChunkSize = chunkSize;
		WorldReference = worldReference;
		WorldPosition = worldPosition;
		Grid = new(chunkSize, Vector3.one, worldPosition);
	}

	public MeshData GetMeshData()
	{
		MeshData meshData = new MeshData();

		foreach (var pos in Grid.Bounds.allPositionsWithin)
		{
			ChunkMeshHelper.GenerateMeshDataForPosition(this, pos, meshData, Grid.GetValue(pos));
		}
		
		return meshData;
	}
}
