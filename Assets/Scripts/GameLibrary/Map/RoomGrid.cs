using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLibrary.Map
{
    public class RoomGrid
    {
        private Vector3 _northEastUp;
        private Vector3 _southWestDown;
        private float _nodeRadius;
        private Node[,] _grid;
        private float _nodeDiameter;
        private int _widthInMeters;
        private int _depthInMeters;
        private int _widthInNodes;
        private int _depthInNodes;
        private Vector3 _centerFloor;
        private int _roomId;
        public int maxSize { get { return _widthInNodes * _depthInNodes; } }


        public RoomGrid(int roomId, Vector3 northEastUp, Vector3 southWestDown, float nodeRadius)
        {
            _roomId = roomId;
            _northEastUp = northEastUp;
            _southWestDown = southWestDown;
            _widthInMeters = Mathf.RoundToInt(_northEastUp.x - _southWestDown.x);
            _depthInMeters = Mathf.RoundToInt(_northEastUp.z - _southWestDown.z);

            _centerFloor = new Vector3(_southWestDown.x + (_widthInMeters / 2), _southWestDown.y, _southWestDown.z + (_depthInMeters / 2));
            _nodeRadius = nodeRadius;
            _nodeDiameter = _nodeRadius * 2;

            _widthInNodes = Mathf.RoundToInt(_widthInMeters / _nodeDiameter);
            _depthInNodes = Mathf.RoundToInt(_depthInMeters / _nodeDiameter);

            CreateGrid();
        }
        public void DrawGizmos()
        {
            Gizmos.DrawWireCube(_centerFloor, new Vector3(_widthInMeters, 1, _depthInMeters));

            if(_grid != null)
            {
                if (MainMap.GetCurrentRoom().id == _roomId)
                {
                    foreach (Node n in _grid)
                    {
                        float cubeSize = _nodeDiameter * 0.9f;
                        Gizmos.color = (n.isWalkable) ? Color.white : Color.red;
                        Gizmos.DrawCube(n.worldPosition, new Vector3(cubeSize, cubeSize, cubeSize));
                    }
                }
            }
        }
        public List<Node> GetNodeNeighbors(Node n)
        {
            List<Node> neighbors = new List<Node>();
            for(int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue;

                    int checkX = n.gridX + x;
                    int checkZ = n.gridZ + z;

                    if(checkX >= 0 && checkX < _widthInNodes && checkZ >= 0 && checkZ < _depthInNodes)
                    {
                        neighbors.Add(_grid[checkX, checkZ]);
                    }
                }
            }
            return neighbors;
        }

        public Node GetNodeFromWorldPosition(Vector3 position)
        {
            float percentX = Mathf.Clamp01((position.x - _southWestDown.x) / _widthInMeters);
            float percentZ = Mathf.Clamp01((position.z - _southWestDown.z) / _depthInMeters);
            int x = Mathf.RoundToInt((_widthInNodes - 1) * percentX);
            int z = Mathf.RoundToInt((_depthInNodes - 1) * percentZ);
            return _grid[x, z];
        }
        public void UpdateGrid()
        {
            if(_grid == null)
            {
                CreateGrid();
                return;
            }


            for (int x = 0; x < _widthInNodes; x++)
            {
                for (int z = 0; z < _depthInNodes; z++)
                {
                    Node n = _grid[x, z];
                    //bool isWalkable = (Physics.CheckSphere(worldPoint, _nodeRadius, unwalkableLayer)) ? false : true;
                    bool isWalkable = true;
                    try
                    {
                        Collider[] hitColliders = Physics.OverlapSphere(n.worldPosition, _nodeRadius);
                        foreach (Collider c in hitColliders)
                        {
                            if (c.gameObject.layer == LayerMask.NameToLayer(GlobalMapParameters.unwalkableLayerName))
                            {
                                isWalkable = false;
                                break;
                            }
                        }
                        n.isWalkable = isWalkable;
                    }
                    catch (System.StackOverflowException ex)
                    {
                        Debug.LogError(string.Format("Stack overflow exception in {0}, {1}", x, z));

                    }
                }
            }

        }
        private void CreateGrid()
        {
            _grid = new Node[_widthInNodes, _depthInNodes];
            //LayerMask unwalkableLayer = LayerMask.NameToLayer(GlobalMapParameters.unwalkableLayerName);
            for (int x = 0; x < _widthInNodes; x++)
            {
                for (int z = 0; z < _depthInNodes; z++)
                {
                    Vector3 worldPoint = _southWestDown 
                        + (Vector3.right * (x * _nodeDiameter + _nodeRadius)) 
                        + (Vector3.forward * (z * _nodeDiameter + _nodeRadius));


                    //bool isWalkable = (Physics.CheckSphere(worldPoint, _nodeRadius, unwalkableLayer)) ? false : true;
                    bool isWalkable = true;
                    Collider[] hitColliders = Physics.OverlapSphere(worldPoint, _nodeRadius);
                    foreach(Collider c in hitColliders)
                    {
                        if(c.gameObject.layer == LayerMask.NameToLayer(GlobalMapParameters.unwalkableLayerName))
                        {
                            isWalkable = false;
                            break;
                        }
                    }
                    _grid[x, z] = new Node(isWalkable, worldPoint, x, z);
                }
            }

        }

    }
}
