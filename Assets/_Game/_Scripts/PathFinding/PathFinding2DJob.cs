using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace DOTSPathfinding.TwoD
{
    public struct PathNode2D
    {
        public int X;
        public int Y;
        public int Index;

        public int GCost; //Cost from the start node to this node
        public int HCost; //Heuristic cost from, this node to the end node
        public int FCost; //G + H
        public int CameFromNodeIndex; //Which node we came from in the current path

        public bool IsWalkable;

        public void CalculateFCost() => FCost = GCost + HCost;
        public int2 GetPosition() => new (X, Y);
    }

    [BurstCompile]
    public struct PathFinding2DJob : IJob
    {
        #region Fields

        private const int MOVE_COST_STRAIGHT = 10;
        private const int MOVE_COST_DIAGONAL = 14;

        public int2 StartPos;
        public int2 EndPos;
        public int2 GridSize;
        public bool AllowDiagonal;
        public NativeList<int2> CalculatedPath;

        #endregion

        #region Public Methods

        public void Execute()
        {
            FindPath(StartPos, EndPos, GridSize);
        }

        #endregion

        #region Private Methods

        private void FindPath(int2 startPos, int2 endPos, int2 gridSize)
        {
            //Test Grid
            NativeArray<PathNode2D> nodeArray = new(gridSize.x * gridSize.y, Allocator.Temp);

            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    var node = new PathNode2D
                    {
                        X = x,
                        Y = y,
                        Index = CalculateIndex(x, y, gridSize),
                        GCost = int.MaxValue,
                        HCost = CalculateHeuristicCost(new int2(x, y), endPos),
                        IsWalkable = true, //TODO replace with some condition, like checking if the grid is occupied
                        CameFromNodeIndex = -1 //-1 means this node is currently invalid. THis will be modified when calculating the path
                    };

                    node.CalculateFCost();
                    nodeArray[node.Index] = node;
                }
            }

            

            var startNode = nodeArray[CalculateIndex(startPos.x, startPos.y, gridSize)];
            startNode.GCost = 0;
            startNode.CalculateFCost();
            nodeArray[startNode.Index] = startNode;

            var endNodeIndex = CalculateIndex(endPos.x, endPos.y, gridSize);

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

        private void TraverseGrid(NativeArray<PathNode2D> nodeArray, NativeList<int> openList, NativeList<int> closedList, int endNodeIndex, int2 gridSize)
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
            NativeArray<PathNode2D> nodeArray, 
            int2 gridSize, 
            PathNode2D currentNode2D, 
            int currentNodeIndex, 
            int2 neighbourPos
            )
        {
            if (!IsPositionInsideGrid(neighbourPos, gridSize))
                return;

            var neighbourIndex = CalculateIndex(neighbourPos.x, neighbourPos.y, gridSize);

            if (closedList.Contains(neighbourIndex))
                return;

            var neighbourNode = nodeArray[neighbourIndex];

            if (!neighbourNode.IsWalkable)
                return;

            var tentativeGCost = currentNode2D.GCost + CalculateHeuristicCost(currentNode2D.GetPosition(), neighbourPos);

            if (tentativeGCost >= neighbourNode.GCost)
                return;
                
            neighbourNode.CameFromNodeIndex = currentNodeIndex;
            neighbourNode.GCost = tentativeGCost;
            neighbourNode.CalculateFCost();
            nodeArray[neighbourIndex] = neighbourNode;

            if (!openList.Contains(neighbourNode.Index))
                openList.Add(neighbourNode.Index);
        }

        private int CalculateIndex(int x, int y, int2 gridSize)
        {
            return x + y * gridSize.x;
        }

        private int CalculateHeuristicCost(int2 posA, int2 posB)
        {
            var xDistance = math.abs(posA.x - posB.x);
            var yDistance = math.abs(posA.y - posB.y);
            
            if (AllowDiagonal)
            {
                var remaining = math.abs(xDistance - yDistance);
                return MOVE_COST_DIAGONAL * math.min(xDistance, yDistance) + MOVE_COST_STRAIGHT * remaining;
            }

            return (xDistance + yDistance) * MOVE_COST_STRAIGHT;
        }

        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode2D> nodeArray)
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

        private bool IsPositionInsideGrid(int2 gridPos, int2 gridSize)
        {
            return
                gridPos.x >= 0 &&
                gridPos.y >= 0 &&
                gridPos.x < gridSize.x &&
                gridPos.y < gridSize.y;
        }

        private void RetracePath(NativeArray<PathNode2D> nodeArray, NativeList<int2> path, int endNodeIndex)
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

        private NativeArray<int2> GetNeighboursOffsetArray()
        {
            NativeArray<int2> offsets = new(AllowDiagonal ? 8 : 4, Allocator.Temp);
            
            offsets[0] = new int2(-1, 0);
            offsets[1] = new int2(+1, 0);
            offsets[2] = new int2(0, -1);
            offsets[3] = new int2(0, +1);

            if (AllowDiagonal)
            {
                offsets[4] = new int2(-1, -1);
                offsets[5] = new int2(-1, +1);
                offsets[6] = new int2(+1, -1);
                offsets[7] = new int2(+1, +1);
            }

            return offsets;
        }

        #endregion
    }
}