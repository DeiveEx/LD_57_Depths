using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace DOTSPathfinding.ThreeD
{
    public struct PathNode3D
    {
        public int X;
        public int Y;
        public int Z;
        public int Index;

        public int GCost; //Cost from the start node to this node
        public int HCost; //Heuristic cost from, this node to the end node
        public int FCost; //G + H
        public int CameFromNodeIndex; //Which node we came from in the current path

        public bool IsWalkable;

        public void CalculateFCost() => FCost = GCost + HCost;
        public int3 GetPosition() => new(X, Y, Z);
    }

    [BurstCompile]
    public struct FindPath3DJob : IJob
    {
        #region Fields

        private const int MOVE_COST_STRAIGHT = 10;
        private const int MOVE_COST_DIAGONAL = 14;

        public int3 StartPos;
        public int3 EndPos;
        public int3 GridSize;
        public bool AllowDiagonal;
        public NativeList<int3> CalculatedPath;

        #endregion

        #region Public Methods

        public void Execute()
        {
            FindPath(StartPos, EndPos, GridSize);
        }

        #endregion

        #region Private Methods

        private void FindPath(int3 startPos, int3 endPos, int3 gridSize)
        {
            //TODO something is broken when diagonalsare enabled

            //Test Grid
            NativeArray<PathNode3D> nodeArray = new(gridSize.x * gridSize.y * gridSize.z, Allocator.Temp);

            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        int3 pos = new(x, y, z);

                        var node = new PathNode3D
                        {
                            X = x,
                            Y = y,
                            Z = z,
                            Index = CalculateIndex(pos, gridSize),
                            GCost = int.MaxValue,
                            HCost = CalculateHeuristicCost(pos, endPos),
                            IsWalkable = true, //TODO replace with some condition, like checking if the grid is occupied
                            CameFromNodeIndex = -1 //-1 means this node is currently invalid. THis will be modified when calculating the path
                        };

                        node.CalculateFCost();
                        nodeArray[node.Index] = node;
                    }
                }
            }

            var startNode = nodeArray[CalculateIndex(startPos, gridSize)];
            startNode.GCost = 0;
            startNode.CalculateFCost();
            nodeArray[startNode.Index] = startNode;

            var endNodeIndex = CalculateIndex(endPos, gridSize);

            NativeList<int> openList = new(Allocator.Temp);
            NativeList<int> closedList = new(Allocator.Temp);

            openList.Add(startNode.Index);

            //Traverse the grid while there's still nods in the open list
            TraverseGrid(nodeArray, openList, closedList, endNodeIndex, gridSize);

            //If path has a length of zero, then there's not path
            RetracePath(nodeArray, CalculatedPath, endNodeIndex);

            nodeArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }

        private void TraverseGrid(NativeArray<PathNode3D> nodeArray, NativeList<int> openList, NativeList<int> closedList, int endNodeIndex, int3 gridSize)
        {
            var neighbourOffsetArray = GetNeighboursOffsetArray();

            while (openList.Length > 0)
            {
                var currentNodeIndex = GetLowestCostFNodeIndex(openList, nodeArray);
                var currentNode = nodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                    break;

                for (var i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentNodeIndex);

                for (var i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    var neighbourPos = currentNode.GetPosition() + neighbourOffsetArray[i];
                    CheckNeighbour(openList, closedList, nodeArray, gridSize, currentNode, currentNodeIndex, neighbourPos);
                }
            }

            neighbourOffsetArray.Dispose();
        }

        private void CheckNeighbour(
            NativeList<int> openList,
            NativeList<int> closedList,
            NativeArray<PathNode3D> nodeArray,
            int3 gridSize,
            PathNode3D currentNode3D,
            int currentNodeIndex,
            int3 neighbourPos
        )
        {
            if (!IsPositionInsideGrid(neighbourPos, gridSize))
                return;

            var neighbourIndex = CalculateIndex(neighbourPos, gridSize);

            if (closedList.Contains(neighbourIndex))
                return;

            var neighbourNode = nodeArray[neighbourIndex];

            if (!neighbourNode.IsWalkable)
                return;

            var tentativeGCost = currentNode3D.GCost + CalculateHeuristicCost(currentNode3D.GetPosition(), neighbourPos);

            if (tentativeGCost >= neighbourNode.GCost)
                return;

            neighbourNode.CameFromNodeIndex = currentNodeIndex;
            neighbourNode.GCost = tentativeGCost;
            neighbourNode.CalculateFCost();
            nodeArray[neighbourIndex] = neighbourNode;

            if (!openList.Contains(neighbourNode.Index))
                openList.Add(neighbourNode.Index);
        }

        private int CalculateIndex(int3 pos, int3 gridSize)
        {
            return (pos.x * gridSize.y * gridSize.z) + (pos.y * gridSize.z) + pos.z;
        }

        private int CalculateHeuristicCost(int3 posA, int3 posB)
        {
            int xDistance = math.abs(posA.x - posB.x);
            int yDistance = math.abs(posA.y - posB.y);
            int zDistance = math.abs(posA.z - posB.z);

            //Manhattan Distance
            return (xDistance + yDistance + zDistance) * MOVE_COST_STRAIGHT;
        }

        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode3D> nodeArray)
        {
            var lowestCostNode = nodeArray[openList[0]];

            for (var i = 1; i < openList.Length; i++)
            {
                var node = nodeArray[openList[i]];

                if (node.FCost < lowestCostNode.FCost)
                    lowestCostNode = node;
            }

            return lowestCostNode.Index;
        }

        private bool IsPositionInsideGrid(int3 gridPos, int3 gridSize)
        {
            return
                gridPos.x >= 0 &&
                gridPos.y >= 0 &&
                gridPos.z >= 0 &&
                gridPos.x < gridSize.x &&
                gridPos.y < gridSize.y &&
                gridPos.z < gridSize.z;
        }

        private void RetracePath(NativeArray<PathNode3D> nodeArray, NativeList<int3> path, int endNodeIndex)
        {
            CalculatedPath.Clear();
            var endNode = nodeArray[endNodeIndex];

            if (endNode.CameFromNodeIndex == -1)
                return;

            path.Add(endNode.GetPosition());
            var currentNode = endNode;

            while (currentNode.CameFromNodeIndex != -1)
            {
                var cameFromNode = nodeArray[currentNode.CameFromNodeIndex];
                path.Add(cameFromNode.GetPosition());
                currentNode = cameFromNode;
            }
        }

        private NativeArray<int3> GetNeighboursOffsetArray()
        {
            NativeArray<int3> offsets = new(AllowDiagonal ? 26 : 6, Allocator.Temp);

            //Cardinal directions
            offsets[0] = new int3(-1, 0, 0);
            offsets[1] = new int3(+1, 0, 0);
            offsets[2] = new int3(0, -1, 0);
            offsets[3] = new int3(0, +1, 0);
            offsets[4] = new int3(0, 0, -1);
            offsets[5] = new int3(0, 0, +1);

            if (!AllowDiagonal)
                return offsets;

            //Diagonals on Y = 0
            offsets[0] = new int3(-1, 0, +1);
            offsets[1] = new int3(+1, 0, +1);
            offsets[0] = new int3(-1, 0, -1);
            offsets[1] = new int3(+1, 0, -1);

            //Diagonals on Y = 1
            offsets[0] = new int3(-1, 1, 0);
            offsets[1] = new int3(+1, 1, 0);
            offsets[4] = new int3(0, 1, -1);
            offsets[5] = new int3(0, 1, +1);
            offsets[0] = new int3(-1, 1, +1);
            offsets[1] = new int3(+1, 1, +1);
            offsets[0] = new int3(-1, 1, -1);
            offsets[1] = new int3(+1, 1, -1);

            //Diagonals on Y = -1
            offsets[0] = new int3(-1, -1, 0);
            offsets[1] = new int3(+1, -1, 0);
            offsets[4] = new int3(0, -1, -1);
            offsets[5] = new int3(0, -1, +1);
            offsets[0] = new int3(-1, -1, +1);
            offsets[1] = new int3(+1, -1, +1);
            offsets[0] = new int3(-1, -1, -1);
            offsets[1] = new int3(+1, -1, -1);

            return offsets;
        }

        #endregion
    }
}