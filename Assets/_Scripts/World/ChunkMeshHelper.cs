using UnityEngine;

public static class ChunkMeshHelper
{
	private static Direction[] _directions =
	{
		Direction.Up,
		Direction.Down,
		Direction.Left,
		Direction.Right,
		Direction.Forward,
		Direction.Backwards
	};
	
	/// <summary>
	/// Generates the <see cref="MeshData"/> for a certain position in the chunk.
	/// </summary>
	public static void GenerateMeshDataForPosition(ChunkData chunk, Vector3Int position, MeshData meshData, BlockType blockType)
	{
		if (blockType == BlockType.Empty)
			return;

		//Looks through all 6 directions
		foreach (var direction in _directions)
		{
			//Get the neighbour block in the current direction
			var neighbourBlockPosition = position + direction.GetVector();
			
			//TODO Here we would check if the block is in a different chunk. For now let's just ignore it
			// var neighbourBlockType = Chunk.GetBlockFromChunkCoordinates(chunk, neighbourBlockPosition);
			chunk.Grid.TryGetValue(neighbourBlockPosition, out var neighbourBlockType);

			//Check if we should render a face for the current direction in this voxel.
			if (neighbourBlockType == BlockType.Empty)
				PopulateFaceData(direction, position, meshData);
		}
	}
	
	/// <summary>
	/// Constructs a single face facing a certain direction
	/// </summary>
	private static void PopulateFaceData(Direction direction, Vector3Int position, MeshData meshData)
	{
		PopulateFaceVertices(direction, position, meshData);
		meshData.AddQuadTriangles();
		// meshData.UV.AddRange(GetFaceUVs(direction, blockType));
	}

	private static void PopulateFaceVertices(Direction direction, Vector3Int position, MeshData data)
	{
		switch (direction)
		{
			case Direction.Up:
				data.AddVertex(new Vector3(position.x + 1f, position.y + 1f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 1f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 1f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 1f, position.z + 1f));
				break;
			case Direction.Down:
				data.AddVertex(new Vector3(position.x + 0f, position.y + 0f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 0f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 0f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 0f, position.z + 1f));
				break;
			case Direction.Left:
				data.AddVertex(new Vector3(position.x + 0f, position.y + 0f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 1f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 1f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 0f, position.z + 0f));
				break;
			case Direction.Right:
				data.AddVertex(new Vector3(position.x + 1f, position.y + 0f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 1f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 1f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 0f, position.z + 1f));
				break;
			case Direction.Forward:
				data.AddVertex(new Vector3(position.x + 1f, position.y + 0f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 1f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 1f, position.z + 1f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 0f, position.z + 1f));
				break;
			case Direction.Backwards:
				data.AddVertex(new Vector3(position.x + 0f, position.y + 0f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 0f, position.y + 1f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 1f, position.z + 0f));
				data.AddVertex(new Vector3(position.x + 1f, position.y + 0f, position.z + 0f));
				break;
		}
	}

	// private static Vector2Int TexturePosition(Direction direction, BlockType blockType)
	// {
	// 	return direction switch {
	// 		Direction.Up => BlockDataManager.blockTextureDataDictionary[blockType].up,
	// 		Direction.Down => BlockDataManager.blockTextureDataDictionary[blockType].down,
	// 		_ => BlockDataManager.blockTextureDataDictionary[blockType].side,
	// 	};
	// }
	//
	// private static Vector2[] GetFaceUVs(Direction direction, BlockType blockType)
	// {
	// 	Vector2[] uvs = new Vector2[4]; //A face has 4 vertices, so we need 4 UVs
	// 	var tilePos = TexturePosition(direction, blockType);
	//
	// 	float x = BlockDataManager.tileSize.x * tilePos.x;
	// 	float y = BlockDataManager.tileSize.y * tilePos.y;
	//
	// 	uvs[0] = new Vector2(x + BlockDataManager.tileSize.x - BlockDataManager.textureOffset,
	// 		y + BlockDataManager.textureOffset);
	//
	// 	uvs[1] = new Vector2(x + BlockDataManager.tileSize.x - BlockDataManager.textureOffset,
	// 		y + BlockDataManager.tileSize.y - BlockDataManager.textureOffset);
	//
	// 	uvs[2] = new Vector2(x + BlockDataManager.textureOffset,
	// 		y + BlockDataManager.tileSize.y - BlockDataManager.textureOffset);
	//
	// 	uvs[3] = new Vector2(x + BlockDataManager.textureOffset,
	// 		y + BlockDataManager.textureOffset);
	//
	// 	return uvs;
	// }
}
