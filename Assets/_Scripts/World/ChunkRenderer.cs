using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
	[SerializeField] private MeshFilter _meshFilter;
	[SerializeField] private MeshRenderer _meshRenderer;
	[SerializeField] private MeshCollider _meshCollider;
	[SerializeField] private bool _showGizmos;
	
	private Mesh _mesh;
	private bool _isDirty = false; //Used to avoid regenerating the mesh multiple times in a single frame

	public ChunkData ChunkData { get; private set; }

	public bool ModifiedByPlayer
	{
		get => ChunkData.IsModified;
		set => ChunkData.IsModified = value;
	}

	private void Awake()
	{
		_mesh = _meshFilter.mesh;
	}

	public void Initialize(ChunkData data)
	{
		ChunkData = data;
	}

	public void UpdateChunk()
	{
		if(!_isDirty || ChunkData == null)
			return;
		
		RenderMesh(ChunkData.GetMeshData());
		_isDirty = false;
	}

	private void RenderMesh(MeshData data)
	{
		//Sets the visual mesh
		_mesh.Clear();
		_mesh.SetVertices(data.Vertices);
		_mesh.SetTriangles(data.Triangles, 0);
		_mesh.SetUVs(0, data.UV);
		_mesh.RecalculateNormals();

		//Sets the collision mesh
		_meshCollider.sharedMaterial = null;
		_meshCollider.sharedMesh = _mesh;
	}
	
	public void SetDirty()
	{
		_isDirty = true;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if(!_showGizmos || !Application.isPlaying || ChunkData == null)
			return;
		
		Gizmos.color = Selection.activeGameObject == gameObject ? 
			new Color(0, 1, 0, .25f) : 
			new Color(1, 0, 1, .25f);

		var pos = transform.position + new Vector3(ChunkData.ChunkSize.x / 2f, ChunkData.ChunkSize.y / 2f, ChunkData.ChunkSize.z / 2f);
		Gizmos.DrawWireCube(pos, ChunkData.ChunkSize);
	}
#endif
}
