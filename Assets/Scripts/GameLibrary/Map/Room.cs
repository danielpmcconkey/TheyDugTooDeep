using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;

namespace GameLibrary.Map
{
    public class Room : IEquatable<Room>
    {
        #region public members
        public int id;
        //public List<RoomAdjacency> adjacentRooms { get { return _adjacentRooms; } set { _adjacentRooms = value; } }
        //public List<Room> connectedRooms { get { return _connectedRooms; } }
        //public bool isDrawn { get { return _isDrawn; } set { _isDrawn = value; } }
        #endregion public members

        #region geographic members
        protected Vector3 _southWestDown;     // the point in the room with the least x, least y, and least z
        protected Vector3 _northEastUp;       // the point in the room with the most x, most y, and most z
        protected float _easternEdgeX;
        protected float _westernEdgeX;
        protected float _northernEdgeZ;
        protected float _southernEdgeZ;
        protected float _floorY;
        protected float _ceilingY;
        #endregion geographic members

        #region skin members
        private float _tileWidth;           // distance along the x axis
        private float _tileDepth;           // distance along the z axis
        private float _tileHeight;          // distance along the y axis
        private float _tileColorR;
        private float _tileColorG;
        private float _tileColorB;

        private float _brickWidth;           // distance along the x axis
        private float _brickDepth;           // distance along the z axis
        private float _brickHeight;          // distance along the y axis
        private float _brickColorR;
        private float _brickColorG;
        private float _brickColorB;
        #endregion skin members


        private List<RoomAdjacency> _adjacentRooms;
        private List<Room> _connectedRooms;
        private bool _isDrawn;
        private List<Teleporter> _teleportersOut;
        private List<Teleporter> _teleportersIn;



        public Room(int id, Vector3 southWestDown, Vector3 northEastUp, int tileSet)
        {
            this.id = id;

            _isDrawn = false;

            _southWestDown = southWestDown;
            _northEastUp = northEastUp;

            _easternEdgeX = _northEastUp.x;
            _westernEdgeX = _southWestDown.x;
            _northernEdgeZ = _northEastUp.z;
            _southernEdgeZ = _southWestDown.z;
            _floorY = _southWestDown.y;
            _ceilingY = _northEastUp.y;



            switch (tileSet)
            {
                case 1:
                default:
                    _tileWidth = GlobalBuildingMaterials.tile001Width;
                    _tileDepth = GlobalBuildingMaterials.tile001Depth;
                    _tileHeight = GlobalBuildingMaterials.tile001Height;
                    _tileColorR = GlobalBuildingMaterials.tile001ColorR;
                    _tileColorG = GlobalBuildingMaterials.tile001ColorG;
                    _tileColorB = GlobalBuildingMaterials.tile001ColorB;
                    _brickWidth = GlobalBuildingMaterials.brick001Width;
                    _brickDepth = GlobalBuildingMaterials.brick001Depth;
                    _brickHeight = GlobalBuildingMaterials.brick001Height;
                    _brickColorR = GlobalBuildingMaterials.brick001ColorR;
                    _brickColorG = GlobalBuildingMaterials.brick001ColorG;
                    _brickColorB = GlobalBuildingMaterials.brick001ColorB;
                    break;
            }

        }

        #region public interface methods
        public void AddAdjacency(RoomAdjacency a)
        {
            if(_adjacentRooms == null) _adjacentRooms = new List<RoomAdjacency>();

            if (!_adjacentRooms.Contains(a))
                _adjacentRooms.Add(a);
        }
        public void AddConnection(Room r)
        {
            if(_connectedRooms == null) _connectedRooms = new List<Room>();
            if (!_connectedRooms.Contains(r))
                _connectedRooms.Add(r);
        }
        public void AddTeleporter(Teleporter t, bool outbound)
        {
            if (outbound)
            {
                if (_teleportersOut == null) _teleportersOut = new List<Teleporter>();
                _teleportersOut.Add(t);
            }
            else
            {
                if (_teleportersIn == null) _teleportersIn = new List<Teleporter>();
                _teleportersIn.Add(t);
            }
        }
        public void DrawRoom()
        {
            DrawFloor();
            DrawWalls();
            DrawColumns();
            _isDrawn = true;
        }
        public List<RoomAdjacency> GetAdjacencies()
        {
            if (_adjacentRooms == null) _adjacentRooms = new List<RoomAdjacency>();
            return _adjacentRooms;
        }
        public List<Room> GetConnections()
        {
            if (_connectedRooms == null) _connectedRooms = new List<Room>();
            return _connectedRooms;
        }
        public float GetEdge(Direction direction)
        {
            if (direction == Direction.NORTH) return _northernEdgeZ;
            if (direction == Direction.EAST) return _easternEdgeX;
            if (direction == Direction.SOUTH) return _southernEdgeZ;
            if (direction == Direction.WEST) return _westernEdgeX;
            return 0;
        }
        public bool IsDrawn()
        {
            return _isDrawn;
        }
        #endregion public methods


        #region comparison methods
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Room objAsRoom = obj as Room;
            if (objAsRoom == null) return false;
            else return Equals(objAsRoom);
        }
        public bool Equals(Room r)
        {
            if (r == null) return false;
            return (id.Equals(r.id));
        }
        public override int GetHashCode()
        {
            return id;
        }
        #endregion comparison methods



        private void DrawFloor()
        {
            bool offset = false;
            for (float x = _westernEdgeX; x <= _easternEdgeX; x += _tileWidth)
            {
                for (float z = _southernEdgeZ; z <= _northernEdgeZ; z += _tileDepth)
                {
                    float zOffset = 0;
                    if (offset) zOffset = _tileDepth / 2;
                    Transform thisTile = SetTile(new Vector3(x, _floorY, z - zOffset));
                    SetColor(thisTile);
                }
                offset = (offset) ? false : true;
            }

        }
        protected void DrawWall(Vector3 begin, Vector3 end, Direction direction)
        {
            float yCenterOffset = _brickHeight / 2;
            bool brickOffset = false; // sets the alternating brick pattern

            for (float yPosition = begin.y + yCenterOffset; yPosition < end.y + yCenterOffset; yPosition += _brickHeight)
            {
                switch (direction)
                {
                    case Direction.NORTH:
                        // northbound walls go from begin.z to end.z
                        float xPosition = begin.x; // center of the brick on the line
                        // how many bricks will it take?
                        float totalNortherlyDistance = Math.Abs(end.z - begin.z);
                        int numNortherlyBricks = (int)Math.Ceiling(totalNortherlyDistance / _brickDepth); // ceiling rounds always-up, so 7.1 bricks becomes 8
                        if (!brickOffset)
                        {
                            for (int i = 0; i < numNortherlyBricks; i++)
                            {
                                float zCenterOffset = _brickDepth / 2; // you don't want the center of the brick on the beginnig line, you want the edge
                                Vector3 brickPosition = new Vector3(xPosition, yPosition, begin.z + zCenterOffset + (i * _brickDepth));
                                Transform thisBrick = SetBrick(brickPosition, Quaternion.Euler(90, 0, 0));
                            }
                        }
                        else
                        {
                            // place half brick
                            Transform beginBrick = SetBrick(
                                new Vector3(xPosition, yPosition, begin.z),
                                Quaternion.Euler(90, 0, 180),
                                true);
                            //place all the full bricks
                            for (int i = 0; i < numNortherlyBricks - 1; i++) // one fewer full bricks due to the half bricks on either end
                            {
                                float zCenterOffset = _brickDepth; // 1/2 brick depth for the beginning half brick; 1/2 brick depth for the fact that the pointer is in the middle of the brick
                                Vector3 brickPosition = new Vector3(xPosition, yPosition, begin.z + zCenterOffset + (i * _brickDepth));
                                Transform thisBrick = SetBrick(brickPosition, Quaternion.Euler(90, 0, 0));
                            }
                            // place half brick
                            Transform endBrick = SetBrick(
                                new Vector3(xPosition, yPosition, end.z),
                                Quaternion.Euler(90, 0, 0),
                                true);
                        }
                        break;
                    case Direction.EAST:
                        // eastbound walls go from begin.x to end.x
                        float zPosition = begin.z; // center of the brick on the line
                        // how many bricks will it take?
                        float totalEasterlyDistance = Math.Abs(end.x - begin.x);
                        int numEasterlyBricks = (int)Math.Ceiling(totalEasterlyDistance / _brickDepth); // ceiling rounds always-up, so 7.1 bricks becomes 8
                        if (brickOffset) // swapping offsets for better corners 
                        {
                            for (int i = 0; i < numEasterlyBricks; i++)
                            {
                                float xCenterOffset = _brickDepth / 2; // you don't want the center of the brick on the beginnig line, you want the edge
                                Vector3 brickPosition = new Vector3(begin.x + xCenterOffset + (i * _brickDepth), yPosition, zPosition);
                                Transform thisBrick = SetBrick(brickPosition, Quaternion.Euler(90, 0, 90));
                            }
                        }
                        else
                        {
                            // place half brick
                            Transform beginBrick = SetBrick(
                                new Vector3(begin.x, yPosition, zPosition),
                                Quaternion.Euler(90, 0, 90),
                                true);
                            //place all the full bricks
                            for (int i = 0; i < numEasterlyBricks - 1; i++) // one fewer full bricks due to the half bricks on either end
                            {
                                float xCenterOffset = _brickDepth; // 1/2 brick depth for the beginning half brick; 1/2 brick depth for the fact that the pointer is in the middle of the brick
                                Vector3 brickPosition = new Vector3(begin.x + xCenterOffset + (i * _brickDepth), yPosition, zPosition);
                                Transform thisBrick = SetBrick(brickPosition, Quaternion.Euler(90, 0, 90));
                            }
                            // place half brick
                            Transform endBrick = SetBrick(
                                new Vector3(end.x, yPosition, zPosition),
                                //new Vector3(xPosition, yPosition, end.z),
                                Quaternion.Euler(90, 0, 270),
                                true);
                        }
                        break;
                }
                brickOffset = (brickOffset) ? false : true;
            }
        }

        protected virtual void DrawWalls()
        {
            Vector3 nwUp = new Vector3(_westernEdgeX, _ceilingY, _northernEdgeZ);
            Vector3 nwDown = new Vector3(_westernEdgeX, _floorY, _northernEdgeZ);
            Vector3 seDown = new Vector3(_easternEdgeX, _floorY, _southernEdgeZ);
            Vector3 seUp = new Vector3(_easternEdgeX, _ceilingY, _southernEdgeZ);





            // draw west wall; SW -> NW
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(_southWestDown, nwUp, Direction.NORTH); // just draw the wall from corner to corner
            else
            {
                List<float> wallPoints = new List<float>();
                wallPoints.Add(_southernEdgeZ);
                for (int i = 0; i < _adjacentRooms.Count; i++)
                {
                    if (_adjacentRooms[i].adjacentWall == Direction.WEST)
                    {
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.SOUTH));
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.NORTH));
                    }
                }
                wallPoints.Add(_northernEdgeZ);
                if (wallPoints.Count > 2) wallPoints.Sort(); // prevents going out of order
                for (int j = 0; j < wallPoints.Count; j += 2) // take the wall points in pairs
                {
                    Vector3 begin = new Vector3(_westernEdgeX, _floorY, wallPoints[j]);
                    Vector3 end = new Vector3(_westernEdgeX, nwUp.y, wallPoints[j + 1]);
                    DrawWall(begin, end, Direction.NORTH);
                }
            }


            // draw north wall; NW -> NE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(nwDown, _northEastUp, Direction.EAST); // just draw the wall from corner to corner
            else
            {
                List<float> wallPoints = new List<float>();
                wallPoints.Add(_westernEdgeX);
                for(int i = 0; i < _adjacentRooms.Count; i++)
                {
                    if(_adjacentRooms[i].adjacentWall == Direction.NORTH)
                    {
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.WEST));
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.EAST));
                    }
                }
                wallPoints.Add(_easternEdgeX);
                if (wallPoints.Count > 2) wallPoints.Sort(); // prevents going out of order
                for(int j = 0; j < wallPoints.Count; j+=2) // take the wall points in pairs
                {
                    Vector3 begin = new Vector3(wallPoints[j], _floorY, _northernEdgeZ);
                    Vector3 end = new Vector3(wallPoints[j+1], _ceilingY, _northernEdgeZ);
                    DrawWall(begin, end, Direction.EAST);
                }
            }

            // draw east wall; SE -> NE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(seDown, _northEastUp, Direction.NORTH); // just draw the wall from corner to corner
            else
            {
                List<float> wallPoints = new List<float>();
                wallPoints.Add(_southernEdgeZ);
                for (int i = 0; i < _adjacentRooms.Count; i++)
                {
                    if (_adjacentRooms[i].adjacentWall == Direction.EAST)
                    {
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.SOUTH));
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.NORTH));
                    }
                }
                wallPoints.Add(_northernEdgeZ);
                if (wallPoints.Count > 2) wallPoints.Sort(); // prevents going out of order
                for (int j = 0; j < wallPoints.Count; j += 2) // take the wall points in pairs
                {
                    Vector3 begin = new Vector3(_easternEdgeX, _floorY, wallPoints[j]);
                    Vector3 end = new Vector3(_easternEdgeX, _ceilingY, wallPoints[j + 1]);
                    DrawWall(begin, end, Direction.NORTH);
                }
            }

            // draw south wall; SW -> SE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(_southWestDown, seUp, Direction.EAST); // just draw the wall from corner to corner
            else
            {
                List<float> wallPoints = new List<float>();
                wallPoints.Add(_westernEdgeX);
                for (int i = 0; i < _adjacentRooms.Count; i++)
                {
                    if (_adjacentRooms[i].adjacentWall == Direction.SOUTH)
                    {
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.WEST));
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.EAST));
                    }
                }
                wallPoints.Add(_easternEdgeX);
                if (wallPoints.Count > 2) wallPoints.Sort(); // prevents going out of order
                for (int j = 0; j < wallPoints.Count; j += 2) // take the wall points in pairs
                {
                    Vector3 begin = new Vector3(wallPoints[j], _floorY, _southernEdgeZ);
                    Vector3 end = new Vector3(wallPoints[j + 1], seUp.y, _southernEdgeZ);
                    DrawWall(begin, end, Direction.EAST);
                }
            }
        }

        private void DrawColumns()
        {
            SetColumn(new Vector3(_easternEdgeX, 1 + _tileHeight, _southernEdgeZ), Quaternion.Euler(-90, 0, 0)); // SE column
            SetColumn(new Vector3(_easternEdgeX, 1 + _tileHeight, _northernEdgeZ), Quaternion.Euler(-90, 0, 0)); // NE column
            SetColumn(new Vector3(_westernEdgeX, 1 + _tileHeight, _northernEdgeZ), Quaternion.Euler(-90, 0, 0)); // NW column
            SetColumn(new Vector3(_westernEdgeX, 1 + _tileHeight, _southernEdgeZ), Quaternion.Euler(-90, 0, 0)); // SW column
        }

        private Transform SetTile(Vector3 position)
        {
            Transform thisTile = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.tile001, position, Quaternion.Euler(-90, 0, 0)).AddComponent<BoxCollider>().transform;
            return thisTile;
        }
        private Transform SetBrick(Vector3 position, Quaternion rotation, bool cap = false)
        {
            Transform thisBrick;
            if (cap) thisBrick = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.halfBrick001, position, rotation).AddComponent<BoxCollider>().transform;
            else thisBrick = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.brick001, position, rotation).AddComponent<BoxCollider>().transform;
            return thisBrick;
        }
        private Transform SetColumn(Vector3 position, Quaternion rotation)
        {
            Transform thisColumn = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.column001, position, rotation).AddComponent<BoxCollider>().transform;
            return thisColumn;
        }
        private void SetColor(Transform thisBrick)
        {
            float rModifier = 0;// RNG.getRandomInt(-10, 10);
            float gModifier = 0;//RNG.getRandomInt(-10, 10);
            float bModifier = 0;//RNG.getRandomInt(-10, 10);
            thisBrick.GetComponent<Renderer>().material.color = new Color(_tileColorR + rModifier, _tileColorG + gModifier, _tileColorB + bModifier);
        }

    }
}