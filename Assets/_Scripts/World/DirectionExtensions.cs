using System;
using UnityEngine;

public static class DirectionExtensions
{
	//Extension method to convert the Direction enum to a vector
	public static Vector3Int GetVector(this Direction direction)
	{
		return direction switch {
			Direction.Up => Vector3Int.up,
			Direction.Down => Vector3Int.down,
			Direction.Left => Vector3Int.left,
			Direction.Right => Vector3Int.right,
			Direction.Forward => Vector3Int.forward,
			Direction.Backwards => Vector3Int.back,
			_ => throw new Exception("Invalid input direction")
		};
	}
}
