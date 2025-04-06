using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
	public List<Vector3> Vertices = new();
	public List<int> Triangles = new();
	public List<Vector2> UV = new();

	public void AddVertex(Vector3 vertex)
	{
		Vertices.Add(vertex);
	}

	public void AddQuadTriangles()
	{
		Triangles.Add(Vertices.Count - 4);
		Triangles.Add(Vertices.Count - 3);
		Triangles.Add(Vertices.Count - 2);

		Triangles.Add(Vertices.Count - 4);
		Triangles.Add(Vertices.Count - 2);
		Triangles.Add(Vertices.Count - 1);
	}
}
