using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Map;
using System;

namespace GameLibrary.AI
{
    public struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;
        public RoomGrid grid;
        public MonoBehaviour caller;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> pCallback, RoomGrid pGrid, MonoBehaviour pCaller)
        {
            pathStart = start;
            pathEnd = end;
            callback = pCallback;
            grid = pGrid;
            caller = pCaller;
        }
    }
    public static class Pathfinder
    {
        const int diagonalMoveCost = 14;
        const int lateralMoveCost = 10;
        private static Queue<PathRequest> _requests = new Queue<PathRequest>();
        private static PathRequest _currentRequest;
        private static bool _isProcessing;


        public static IEnumerator RequestPath(PathRequest request)
        {
            _requests.Enqueue(request);
            yield return null;
            TryProcessNextRequest();
        }
        private static void TryProcessNextRequest()
        {
            if (!_isProcessing && _requests.Count > 0)
            {
                _currentRequest = _requests.Dequeue();
                _isProcessing = true;
                //StartFindPath(_currentRequest);
                FindPath(_currentRequest);
            }
        }
        //private static void StartFindPath(PathRequest request)
        //{
        //    StartCoroutine(Pathfinder.FindPath(request, this));

        //}
        public static void FinishedProcessingPath(Vector3[] path, bool sucess, MonoBehaviour caller)
        {
            if (caller != null) // prevents calling back to a killed object
                _currentRequest.callback(path, sucess);
            _isProcessing = false;
            TryProcessNextRequest();
        }


        public static void FindPath(PathRequest request)
        {
            Vector3[] waypoints = new Vector3[0];
            bool isSuccessful = false;

            Node startNode = request.grid.GetNodeFromWorldPosition(request.pathStart);
            Node targetNode = request.grid.GetNodeFromWorldPosition(request.pathEnd);
            if (startNode.isWalkable && targetNode.isWalkable)
            {
                Heap<Node> openSet = new Heap<Node>(request.grid.maxSize);
                HashSet<Node> closedSet = new HashSet<Node>();

                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    Node currentNode = openSet.RemoveFirst();
                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        isSuccessful = true;
                        break;
                    }

                    foreach (Node neighbor in request.grid.GetNodeNeighbors(currentNode))
                    {
                        if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;
                        int newMovementCostToNeighbor = currentNode.gCost + GetDistanceBetweenNodes(currentNode, neighbor);
                        if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                        {
                            neighbor.gCost = newMovementCostToNeighbor;
                            neighbor.hCost = GetDistanceBetweenNodes(neighbor, targetNode);
                            neighbor.parent = currentNode;
                            if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                            else openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }
            //yield return null;
            if(isSuccessful)
            {
                waypoints = RetracePath(startNode, targetNode);
            }
            FinishedProcessingPath(waypoints, isSuccessful, request.caller);
        }
        private static int GetDistanceBetweenNodes(Node a, Node b)
        {
            int xDistance = (a.gridX > b.gridX) ? a.gridX - b.gridX : b.gridX - a.gridX;
            int zDistance = (a.gridZ > b.gridZ) ? a.gridZ - b.gridZ : b.gridZ - a.gridZ;

            if (xDistance == zDistance) return diagonalMoveCost * xDistance;
            if(xDistance < zDistance) return (diagonalMoveCost * xDistance) + (lateralMoveCost * (zDistance - xDistance));
            
            return (diagonalMoveCost * zDistance) + (lateralMoveCost * (xDistance - zDistance));
        }
        private static Vector3[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while(currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;
        }
        
        private static Vector3[] SimplifyPath(List<Node> path)
        {
            if (path == null) return new Vector3[0];

            List<Vector3> waypoints = new List<Vector3>();
            // always add the first node
            if(path.Count >= 1) waypoints.Add(path[0].worldPosition);

            if (path.Count > 1)
            {
                Vector2 diretionOld = Vector2.zero;
                for (int i = 1; i < path.Count; i++)
                {
                    Node n1 = path[i - 1];
                    Node n2 = path[i];
                    Vector2 directionNew = new Vector2(n1.gridX - n2.gridX, n1.gridZ - n2.gridZ);
                    if (directionNew != diretionOld) waypoints.Add(n2.worldPosition);
                    diretionOld = directionNew;
                }
            }
            return waypoints.ToArray();
        }
    }
}
