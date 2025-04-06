using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ignix.GenericGridSystem
{
    public class GridValueChangedEventArgs
    {
        public int X;
        public int Y;
        public int Z;
    }

    public class GenericGrid<T>
    {
        #region Fields

        private readonly T[,,] _gridArray;

        #endregion

        #region Properties

        public Vector3Int Size { get; }
        public Vector3 CellSize { get; }
        public Vector3 Origin { get; }
        public BoundsInt Bounds { get; }

        #endregion
        
        #region Events & Delegates

        public event EventHandler<GridValueChangedEventArgs> GridValueChanged;

        #endregion

        #region Public Methods

        public GenericGrid(
            int width, 
            int height,
            int depth,
            float cellWidth, 
            float cellHeight, 
            float cellDepth, 
            Vector3 origin = new ())
        {
            if (width == 0 || height == 0 || depth == 0)
                throw new InvalidOperationException("You can't have a grid with a dimension set to zero!");
            
            Size = new Vector3Int(width, height, depth);
            CellSize = new Vector3(cellWidth, cellHeight, cellDepth);
            _gridArray = new T[width, height, depth];
            Origin = origin;
            Bounds = new BoundsInt(GetGridPosFromWorldPos(origin), Size);
        }

        public GenericGrid( Vector3Int gridSize, Vector3 cellSize, Vector3 origin) : 
            this(gridSize.x, gridSize.y, gridSize.z, cellSize.x, cellSize.y, cellSize.z, origin){ }
        
        
        public bool IsPositionInsideGrid(int x, int y, int z)
        {
            return x >= 0 && x < Size.x &&
                   y >= 0 && y < Size.y &&
                   z >= 0 && z < Size.z;
        }
        
        public bool IsPositionInsideGrid(Vector3Int gridPos)
        {
            return IsPositionInsideGrid(gridPos.x, gridPos.y, gridPos.z);
        }
        
        public Vector3Int ClampGridPosToGridSize(Vector3Int gridPos)
        {
            gridPos.x = Mathf.Clamp(gridPos.x, 0, Size.x - 1);
            gridPos.y = Mathf.Clamp(gridPos.y, 0, Size.y - 1);
            gridPos.z = Mathf.Clamp(gridPos.z, 0, Size.z - 1);

            return gridPos;
        }
        
        public Vector3Int GetGridPosFromWorldPos(Vector3 worldPos, bool clampToGridSize = true)
        {
            worldPos -= Origin;

            int cellX = Mathf.FloorToInt(worldPos.x / (CellSize.x == 0 ? 1 : CellSize.x));
            int cellY = Mathf.FloorToInt(worldPos.y / (CellSize.y == 0 ? 1 : CellSize.y));
            int cellZ = Mathf.FloorToInt(worldPos.z / (CellSize.z == 0 ? 1 : CellSize.z));

            Vector3Int gridPos = new Vector3Int(cellX, cellY, cellZ);

            if (clampToGridSize)
                gridPos = ClampGridPosToGridSize(gridPos);

            return gridPos;
        }

        public Vector3 GetWorldCellPosition(int x, int y, int z)
        {
            float worldX = x * CellSize.x;
            float worldY = y * CellSize.y;
            float worldZ = z * CellSize.z;
            
            return new Vector3(worldX, worldY, worldZ) + Origin;
        }
        
        public Vector3 GetWorldCellPosition(Vector3Int gridPos)
        {
            return GetWorldCellPosition(gridPos.x, gridPos.y, gridPos.z);
        }

        public Vector3 GetWorldCenterPosition(int x, int y, int z, bool clampToGridSize = true)
        {
            Vector3Int gridPos = new Vector3Int(x, y, z);

            if (clampToGridSize)
                gridPos = ClampGridPosToGridSize(gridPos);

            var worldPos = GetWorldCellPosition(gridPos);

            Vector3 worldCenterPos = new Vector3()
            {
                x = worldPos.x + CellSize.x / 2f,
                y = worldPos.y + CellSize.y / 2f,
                z = worldPos.z + CellSize.z / 2f,
            };

            return worldCenterPos;
        }
        
        public Vector3 GetWorldCenterPosition(Vector3Int gridPos, bool clampToGridSize = true)
        {
            return GetWorldCenterPosition(gridPos.x, gridPos.y, gridPos.z, clampToGridSize);
        }

        public Vector3 GetWorldBasePosition(int x, int y, int z, bool clampToGridSize = true)
        {
            var downVector = CellSize.y == 0 ? Vector3.zero : Vector3.down * (CellSize.y / 2);
            return GetWorldCenterPosition(x, y, z, clampToGridSize) + downVector;
        }
        
        public Vector3 GetWorldBasePosition(Vector3Int gridPos, bool clampToGridSize = true)
        {
            return GetWorldBasePosition(gridPos.x, gridPos.y, gridPos.z, clampToGridSize);
        }

        public T GetValue(int x, int y, int z)
        {
            if (!IsPositionInsideGrid(x, y, z))
                throw new IndexOutOfRangeException($"Cell Position ({x}, {y}, {z}) is outside the grid bounds");

            return _gridArray[x, y, z];
        }
        
        public T GetValue(Vector3Int gridPos)
        {
            return GetValue(gridPos.x, gridPos.y, gridPos.z);
        }

        public bool TryGetValue(Vector3Int gridPos, out T value)
        {
            value = default;
            
            if (!IsPositionInsideGrid(gridPos))
                return false;
            
            value = GetValue(gridPos);
            return true;
        }
        
        public void SetValue(int x, int y, int z, T value)
        {
            if (!IsPositionInsideGrid(x, y, z))
                throw new IndexOutOfRangeException($"Cell Position ({x}, {y}, {z}) is outside the grid bounds");

            _gridArray[x, y, z] = value;
            
            GridValueChanged?.Invoke(this, new GridValueChangedEventArgs() {
                X = x,
                Y = y
            });
        }
        
        public void SetValue(Vector3Int gridPos, T value)
        {
            SetValue(gridPos.x, gridPos.y, gridPos.z, value);
        }

        public void SetValue(Vector3 worldPos, T value)
        {
            Vector3Int gridPos = GetGridPosFromWorldPos(worldPos);
            SetValue(gridPos, value);
        }
        
        public bool TrySetValue(Vector3Int gridPos, T value)
        {
            if (!IsPositionInsideGrid(gridPos))
                return false;

            SetValue(gridPos, value);
            return true;
        }
        
        #endregion
    }
}