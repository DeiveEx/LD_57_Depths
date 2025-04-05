using Ignix.GenericGridSystem;
using UnityEngine;

public class ChunkData
{
	public GenericGrid<BlockType> Grid;
	public Vector3Int ChunkSize;
	public World WorldReference;
	public Vector3Int WorldPosition;

	public bool IsModified = false; //We can use this to decide if we want to save this chunk. Chunks that were not modified will not be saved because we can just regenerate them from scratch.

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
