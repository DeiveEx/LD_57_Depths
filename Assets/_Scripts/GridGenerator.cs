using System.Collections.Generic;
using Ignix.GenericGridSystem;
using UnityEngine;

public enum TileType
{
    Empty,
    Indestructible,
    Rock
}

public class GridGenerator : MonoBehaviour
{
    private static Vector3Int[] _neighborsOffsets = new Vector3Int[]
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.up,
        Vector3Int.down
    };
        
    [SerializeField] private GameObject _rockPrefab;

    private GenericGrid<TileType> _grid;
    private Dictionary<Vector3Int, GameObject> _gridObjects = new(); //Ideally I should be using Marching cubes, not multiple objects

    public GenericGrid<TileType> Grid => _grid;

    public void GenerateGridData(Vector3Int gridSize)
    {
        _grid = new(
            gridSize, 
            Vector3.one,
            Vector3.zero
        );

        foreach (var pos in _grid.Bounds.allPositionsWithin)
        {
            TileType tileType = TileType.Rock;
            _grid.SetValue(pos, tileType);
        }
    }

    public void BuildGrid()
    {
        foreach (var pos in _grid.Bounds.allPositionsWithin)
        {
            CreateTileObject(pos, _grid.GetValue(pos));
        }
        
        UpdateVisibleTiles();
    }

    public void SetTile(Vector3Int gridPos, TileType type)
    {
        if(!_grid.TrySetValue(gridPos, type))
            return;
        
        //Ideally we want to update the visuals only once per frame, so maybe I should store all changes and then update
        //them all at once at the end of the frame or something, specially if I'm using Marching cubes, so I only update
        //the mesh once
        if (type == TileType.Empty)
            RemoveTileObject(gridPos);
        else
            CreateTileObject(gridPos, type);
        
        UpdateVisibleTiles();
    }
    
    private void CreateTileObject(Vector3Int gridPos, TileType type)
    {
        if (type != TileType.Rock)
            return;
        
        var go = Instantiate(_rockPrefab, transform);
        go.transform.position = _grid.GetWorldCenterPosition(gridPos);
        go.name = $"Tile_{gridPos}";
        go.isStatic = true;
        _gridObjects[gridPos] = go;
    }

    private void RemoveTileObject(Vector3Int gridPos)
    {
        if(!_gridObjects.TryGetValue(gridPos, out var go))
            return;
            
        Destroy(go);
        _gridObjects.Remove(gridPos);
    }

    public Vector3Int GetGridPosFromWorldPos(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y),
            Mathf.FloorToInt(worldPos.z)
        );
    }

    public TileType GetTile(Vector3Int gridPos)
    {
        if(_grid.TryGetValue(gridPos, out var tileType))
            return tileType;

        return TileType.Empty;
    }

    private void UpdateVisibleTiles()
    {
        //Only show tiles that have an "empty" tile around them
        foreach (var pos in _grid.Bounds.allPositionsWithin)
        {
            if (_grid.GetValue(pos) == TileType.Empty ||
                !_gridObjects.TryGetValue(pos, out var go))
                continue;
        
            bool hasEmptyNeighbor = false;
            
            foreach (var neighbor in GetGridNeighbors(pos))
            {
                if (_grid.GetValue(neighbor) == TileType.Empty)
                {
                    hasEmptyNeighbor = true;
                    break;
                }
            }
            
            go.SetActive(hasEmptyNeighbor);
        }
    }

    private IEnumerable<Vector3Int> GetGridNeighbors(Vector3Int pos)
    {
        foreach (var offset in _neighborsOffsets)
        {
            var neighbor = pos + offset;
            if (_grid.IsPositionInsideGrid(neighbor))
                yield return neighbor;
        }
    }
}
