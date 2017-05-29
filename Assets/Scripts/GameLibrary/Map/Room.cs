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

        private float _brickWidth;           // distance along the x axis
        private float _brickDepth;           // distance along the z axis
        private float _brickHeight;          // distance along the y axis
        private Color _baseColor;
        #endregion skin members


        private List<RoomAdjacency> _adjacentRooms;
        private List<Room> _connectedRooms;
        private bool _isDrawn;
        private bool _isRendered = false;
        private List<Teleporter> _teleportersOut;
        private List<Teleporter> _teleportersIn;
        private List<Vector3> _torches;
        private List<Transform> _instantiatedObjects;
        private Dictionary<string, Transform> _doors;
        private Transform _container;   // the base container for all objects in this room





        public Room(int id, Vector3 southWestDown, Vector3 northEastUp, int tileSet, Color baseColor)
        {
            this.id = id;

            _isDrawn = false;
            _instantiatedObjects = new List<Transform>();

            _southWestDown = southWestDown;
            _northEastUp = northEastUp;

            _easternEdgeX = _northEastUp.x;
            _westernEdgeX = _southWestDown.x;
            _northernEdgeZ = _northEastUp.z;
            _southernEdgeZ = _southWestDown.z;
            _floorY = _southWestDown.y;
            _ceilingY = _northEastUp.y;

            _baseColor = baseColor;



            switch (tileSet)
            {
                case 1:
                default:
                    _tileWidth = GlobalBuildingMaterials.tile001Width;
                    _tileDepth = GlobalBuildingMaterials.tile001Depth;
                    _tileHeight = GlobalBuildingMaterials.tile001Height;
                    _brickWidth = GlobalBuildingMaterials.brick001Width;
                    _brickDepth = GlobalBuildingMaterials.brick001Depth;
                    _brickHeight = GlobalBuildingMaterials.brick001Height;
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
        public void AddDoor(Transform door)
        {
            if (_doors == null) _doors = new Dictionary<string, Transform>();
            if (!_doors.ContainsKey(door.name))
            {
                _doors.Add(door.name, door);
            }
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
        public void AddTorch(Vector3 v)
        {
            if (_torches == null) _torches = new List<Vector3>();
            _torches.Add(v);
        }
        public virtual void DrawRoom()
        {
            _container = new GameObject().transform;
            _container.name = string.Format("Room {0} container", id); // putting this here because rooms are created to see if they fit and then quickly destroyed

            Transform floorContainer = DrawFloor();
            floorContainer.parent = _container;

            Transform wallsContainer = DrawWalls();
            if (wallsContainer != null) wallsContainer.parent = _container;

            Transform columnsContainer = DrawColumns();
            if(columnsContainer != null) columnsContainer.parent = _container;

            Transform torchesContainer = DrawTorches();
            if (torchesContainer != null) torchesContainer.parent = _container;

            Transform roomEntryTrigger = AddRoomEntryTrigger();
            roomEntryTrigger.parent = _container;

            _isDrawn = true;
            _isRendered = true;
        }
        public virtual void EraseRoom()
        {
            if(_container != null) UnityEngine.Object.Destroy(_container.gameObject);
            foreach (Transform t in _instantiatedObjects)
            {
                if (t.gameObject != null) UnityEngine.Object.Destroy(t.gameObject);
            }
            _instantiatedObjects = new List<Transform>();
            _isDrawn = false;
            _isRendered = false;
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
        public Transform GetContainer()
        {
            return _container;
        }
        public float GetEdge(Direction direction)
        {
            if (direction == Direction.NORTH) return _northernEdgeZ;
            if (direction == Direction.EAST) return _easternEdgeX;
            if (direction == Direction.SOUTH) return _southernEdgeZ;
            if (direction == Direction.WEST) return _westernEdgeX;
            return 0;
        }
        public bool IsCleared()
        {
            return true; // todo: hook IsCleared into actual enemy destruction
        }
        public bool IsDrawn()
        {
            return _isDrawn;
        }
        public bool IsRendered()
        {
            return _isRendered;
        }
        public void LockDoors()
        {
            foreach (KeyValuePair<string, Transform> doorPair in _doors)
            {
                doorPair.Value.SendMessage("LockDoor");
            }
        }
        
        public void RenderRoomObjects(bool isActive)
        {
            foreach (Transform thing in _instantiatedObjects)
            {
                thing.gameObject.SetActive(isActive);
            }
            _isRendered = isActive;
        }
        public void UnlockDoors()
        {
            foreach (KeyValuePair<string, Transform> doorPair in _doors)
            {
                doorPair.Value.SendMessage("UnlockDoor");
            }
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

        private Transform AddRoomEntryTrigger()
        {
            Transform triggerContainer = new GameObject().transform;
            triggerContainer.name = string.Format("Room {0} room trigger container", id);

            float width = _easternEdgeX - _westernEdgeX;
            float depth = _northernEdgeZ - _southernEdgeZ;
            float height = _ceilingY - _floorY;
            float x = _westernEdgeX + (width / 2);
            float y = _floorY + (height / 2);
            float z = _southernEdgeZ + (depth / 2);
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            Vector3 position = new Vector3(x, y, z);

            Transform roomTriggerEmptyShell = GameObject.Instantiate(new GameObject(), position, rotation).transform;
            roomTriggerEmptyShell.parent = triggerContainer;

            roomTriggerEmptyShell.gameObject.AddComponent<BoxCollider>();
            BoxCollider colliderTrigger = (BoxCollider)roomTriggerEmptyShell.GetComponent<Collider>();
            
            colliderTrigger.size = new Vector3(width, _ceilingY - _floorY, depth);
            colliderTrigger.isTrigger = true;
            colliderTrigger.name = string.Format("rigger collider for room ID:{0}", id);
            colliderTrigger.tag = "RoomEntryTrigger";
            


            return triggerContainer;
        }

        private Transform DrawFloor()
        {
            Transform floorContainer = new GameObject().transform;
            floorContainer.name = string.Format("Room {0} floor container", id);
            bool offset = false;
            for (float x = _westernEdgeX; x <= _easternEdgeX; x += _tileWidth)
            {
                for (float z = _southernEdgeZ; z <= _northernEdgeZ; z += _tileDepth)
                {
                    float zOffset = 0;
                    if (offset) zOffset = _tileDepth / 2;
                    Transform thisTile = SetTile(new Vector3(x, _floorY, z - zOffset));
                    thisTile.parent = floorContainer;
                }
                offset = (offset) ? false : true;
            }
            return floorContainer;
        }
        protected Transform DrawWall(Vector3 begin, Vector3 end, Direction direction)
        {
            Transform container = new GameObject().transform;
            container.name = string.Format("Room {0} wall segment container", id);


            //float widthX = (end.x - begin.x == 0) ? GlobalMapParameters.lightBlockerWidth : end.x - begin.x;
            //float centerX = begin.x + (widthX / 2);
            //float widthY = end.y - begin.y;
            //float centerY = begin.y + (widthY / 2);
            //float widthZ = (end.z - begin.z == 0) ? GlobalMapParameters.lightBlockerWidth : end.z - begin.z;
            //float centerZ = begin.z + (widthZ / 2);
            //Transform lightBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            //lightBlocker.parent = container;
            //lightBlocker.position = new Vector3(centerX, centerY, centerZ);
            //lightBlocker.localScale = new Vector3(widthX, widthY, widthZ);


            

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
                                thisBrick.parent = container;
                            }
                        }
                        else
                        {
                            // place half brick
                            Transform beginBrick = SetBrick(
                                new Vector3(xPosition, yPosition, begin.z),
                                Quaternion.Euler(90, 0, 0),
                                true);
                            beginBrick.parent = container;
                            //place all the full bricks
                            for (int i = 0; i < numNortherlyBricks - 1; i++) // one fewer full bricks due to the half bricks on either end
                            {
                                float zCenterOffset = _brickDepth; // 1/2 brick depth for the beginning half brick; 1/2 brick depth for the fact that the pointer is in the middle of the brick
                                Vector3 brickPosition = new Vector3(xPosition, yPosition, begin.z + zCenterOffset + (i * _brickDepth));
                                Transform thisBrick = SetBrick(brickPosition, Quaternion.Euler(90, 0, 0));
                                thisBrick.parent = container;
                            }
                            // place half brick
                            Transform endBrick = SetBrick(
                                new Vector3(xPosition, yPosition, end.z),
                                Quaternion.Euler(90, 0, 180),
                                true);
                            endBrick.parent = container;
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
                                thisBrick.parent = container;
                            }
                        }
                        else
                        {
                            // place half brick
                            Transform beginBrick = SetBrick(
                                new Vector3(begin.x, yPosition, zPosition),
                                Quaternion.Euler(90, 0, 270),
                                true);
                            beginBrick.parent = container;
                            //place all the full bricks
                            for (int i = 0; i < numEasterlyBricks - 1; i++) // one fewer full bricks due to the half bricks on either end
                            {
                                float xCenterOffset = _brickDepth; // 1/2 brick depth for the beginning half brick; 1/2 brick depth for the fact that the pointer is in the middle of the brick
                                Vector3 brickPosition = new Vector3(begin.x + xCenterOffset + (i * _brickDepth), yPosition, zPosition);
                                Transform thisBrick = SetBrick(brickPosition, Quaternion.Euler(90, 0, 90));
                                thisBrick.parent = container;
                            }
                            // place half brick
                            Transform endBrick = SetBrick(
                                new Vector3(end.x, yPosition, zPosition),
                                //new Vector3(xPosition, yPosition, end.z),
                                Quaternion.Euler(90, 0, 90),
                                true);
                            endBrick.parent = container;
                        }
                        break;
                }
                brickOffset = (brickOffset) ? false : true;
            }
            return container;
        }
        protected virtual Transform DrawWalls()
        {
            Transform container = new GameObject().transform;
            container.name = string.Format("Room {0} walls container", id);

            Vector3 nwUp = new Vector3(_westernEdgeX, _ceilingY, _northernEdgeZ);
            Vector3 nwDown = new Vector3(_westernEdgeX, _floorY, _northernEdgeZ);
            Vector3 seDown = new Vector3(_easternEdgeX, _floorY, _southernEdgeZ);
            Vector3 seUp = new Vector3(_easternEdgeX, _ceilingY, _southernEdgeZ);





            // draw west wall; SW -> NW
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
            {
                Transform wallSegment = DrawWall(_southWestDown, nwUp, Direction.NORTH); // just draw the wall from corner to corner
                wallSegment.parent = container;
            }
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
                    Transform wallSegment = DrawWall(begin, end, Direction.NORTH);
                    wallSegment.parent = container;
                }
            }


            // draw north wall; NW -> NE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
            {
                Transform wallSegment = DrawWall(nwDown, _northEastUp, Direction.EAST); // just draw the wall from corner to corner
                wallSegment.parent = container;
            }
            else
            {
                List<float> wallPoints = new List<float>();
                wallPoints.Add(_westernEdgeX);
                for (int i = 0; i < _adjacentRooms.Count; i++)
                {
                    if (_adjacentRooms[i].adjacentWall == Direction.NORTH)
                    {
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.WEST));
                        wallPoints.Add(_adjacentRooms[i].room.GetEdge(Direction.EAST));
                    }
                }
                wallPoints.Add(_easternEdgeX);
                if (wallPoints.Count > 2) wallPoints.Sort(); // prevents going out of order
                for (int j = 0; j < wallPoints.Count; j += 2) // take the wall points in pairs
                {
                    Vector3 begin = new Vector3(wallPoints[j], _floorY, _northernEdgeZ);
                    Vector3 end = new Vector3(wallPoints[j + 1], _ceilingY, _northernEdgeZ);
                    Transform wallSegment = DrawWall(begin, end, Direction.EAST);
                    wallSegment.parent = container;
                }
            }

            // draw east wall; SE -> NE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
            {
                Transform wallSegment = DrawWall(seDown, _northEastUp, Direction.NORTH); // just draw the wall from corner to corner
                wallSegment.parent = container;
            }
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
                    Transform wallSegment = DrawWall(begin, end, Direction.NORTH);
                    wallSegment.parent = container;
                }
            }

            // draw south wall; SW -> SE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
            {
                Transform wallSegment = DrawWall(_southWestDown, seUp, Direction.EAST); // just draw the wall from corner to corner
                wallSegment.parent = container;
            }
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
                    Transform wallSegment = DrawWall(begin, end, Direction.EAST);
                    wallSegment.parent = container;
                }
            }
            return container;
        }
        protected virtual Transform DrawColumns()
        {
            Transform container = new GameObject().transform;
            container.name = string.Format("Room {0} columns container", id);

            Transform column1 = SetColumn(new Vector3(_easternEdgeX, 1 + _tileHeight, _southernEdgeZ), Quaternion.Euler(90, 0, 0)); // SE column
            Transform column2 = SetColumn(new Vector3(_easternEdgeX, 1 + _tileHeight, _northernEdgeZ), Quaternion.Euler(90, 0, 0)); // NE column
            Transform column3 = SetColumn(new Vector3(_westernEdgeX, 1 + _tileHeight, _northernEdgeZ), Quaternion.Euler(90, 0, 0)); // NW column
            Transform column4 = SetColumn(new Vector3(_westernEdgeX, 1 + _tileHeight, _southernEdgeZ), Quaternion.Euler(90, 0, 0)); // SW column
            column1.parent = container;
            column2.parent = container;
            column3.parent = container;
            column4.parent = container;

            return container;
        }
        private Transform DrawTorches()
        {
            Transform container = new GameObject().transform;
            container.name = string.Format("Room {0} torches container", id);

            if (_torches == null) _torches = new List<Vector3>();
            foreach (Vector3 v in _torches)
            {
                Quaternion rotation = Quaternion.Euler(0, 0, 0);

                if (v.z == _northernEdgeZ - GlobalMapParameters.torchWallOffset) // north wall
                    rotation = Quaternion.Euler(-90, -90, 0);
                if (v.z == _southernEdgeZ + GlobalMapParameters.torchWallOffset) // south wall
                    rotation = Quaternion.Euler(-90, 90, 0);
                if (v.x == _easternEdgeX - GlobalMapParameters.torchWallOffset) // east wall
                    rotation = Quaternion.Euler(-90, 0, 0);
                if (v.x == _westernEdgeX + GlobalMapParameters.torchWallOffset) // west wall
                    rotation = Quaternion.Euler(-90, 180, 0);


                Transform torch = SetTorch(v, rotation);
                torch.parent = container;
            }

            return container;
        }

        private Transform SetTile(Vector3 position)
        {
            Transform thisTile = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.tile001, position, Quaternion.Euler(-90, 0, 0)).AddComponent<BoxCollider>().transform;
            _instantiatedObjects.Add(thisTile);
            SetColor(thisTile);
            return thisTile;
        }
        private Transform SetBrick(Vector3 position, Quaternion rotation, bool cap = false)
        {
            Transform thisBrick;
            if (cap) thisBrick = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.halfBrick001, position, rotation).AddComponent<BoxCollider>().transform;
            else thisBrick = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.brick001, position, rotation).AddComponent<BoxCollider>().transform;
            _instantiatedObjects.Add(thisBrick);
            SetColor(thisBrick);
            return thisBrick;
        }
        private Transform SetColumn(Vector3 position, Quaternion rotation)
        {
            Transform thisColumn = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.column001, position, rotation).AddComponent<BoxCollider>().transform;
            _instantiatedObjects.Add(thisColumn);
            SetColor(thisColumn);
            return thisColumn;
        }
        private Transform SetTorch(Vector3 position, Quaternion rotation)
        {
            Transform thisTorch = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.torch001, position, rotation).transform;
            _instantiatedObjects.Add(thisTorch);
            return thisTorch;
        }
        private void SetColor(Transform thisObject)
        {
            float rModifier = (float)RNG.getRandomInt(0, GlobalMapParameters.colorVarianceMax) / 100;
            float gModifier = (float)RNG.getRandomInt(0, GlobalMapParameters.colorVarianceMax) / 100;
            float bModifier = (float)RNG.getRandomInt(0, GlobalMapParameters.colorVarianceMax) / 100;
            thisObject.GetComponent<Renderer>().material.color = new Color(_baseColor.r + rModifier, _baseColor.g + gModifier, _baseColor.b + bModifier);
        }

    }
}