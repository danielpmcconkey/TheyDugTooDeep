using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;
using GameLibrary.Character;

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
        //protected bool[] _grid;        // a grid of "squares" in the room to contain initial object placement
        protected RoomGrid _roomGrid;

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

        public Dictionary<int, Monster> _monsters;
        
        private List<RoomAdjacency> _adjacentRooms;
        private List<Room> _connectedRooms;
        private bool _isDrawn;
        private bool _isRendered = false;
        private bool _isCleared = false;
        private List<Teleporter> _teleporters;
        private List<Vector3> _torches;
        private List<Transform> _instantiatedObjects;
        private Dictionary<string, Transform> _doors;
        private List<DecorationPlaceholder> _decorations;
        private Transform _container;   // the base container for all objects in this room
        private Transform _torchesContainer;
        private Transform _levelMinimapContainer;
        private Transform _roomMinimapContainer;



        public Room(int id, Vector3 southWestDown, Vector3 northEastUp, int tileSet, Color baseColor, Transform levelMinimapContainer)
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

            //InitializeGrid();
            _levelMinimapContainer = levelMinimapContainer;
            _roomMinimapContainer = new GameObject().transform;
            if (_levelMinimapContainer != null) _roomMinimapContainer.parent = _levelMinimapContainer;
            



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
        public DecorationPlaceholder AddDecoration(GameObject gameObject, int northPad = 0, int eastPad = 0, int southPad = 0, int westPad = 0)
        {
            return AddDecoration(gameObject, FindPositionForRoomObject(northPad, eastPad, southPad, westPad), northPad, eastPad, southPad, westPad);
        }
        public DecorationPlaceholder AddDecoration(GameObject gameObject, Vector3 position, int northPad = 0, int eastPad = 0, int southPad = 0, int westPad = 0)
        {
            if (_decorations == null) _decorations = new List<DecorationPlaceholder>();



            List<Vector2> squaresNeeded = new List<Vector2>();
            squaresNeeded.Add(new Vector2(position.x, position.z));
            for (int i = 0; i < northPad; i++) squaresNeeded.Add(new Vector2(position.x, position.z + (i + 1)));
            for (int i = 0; i < southPad; i++) squaresNeeded.Add(new Vector2(position.x, position.z - (i + 1)));
            for (int i = 0; i < eastPad; i++) squaresNeeded.Add(new Vector2(position.x + (i + 1), position.z));
            for (int i = 0; i < westPad; i++) squaresNeeded.Add(new Vector2(position.x - (i + 1), position.z));

            AddRoomSquareOccupant(squaresNeeded);

            DecorationPlaceholder dph = new DecorationPlaceholder() { gameObject = gameObject, position = position };
            _decorations.Add(dph);

            return dph;
        }
        public void AddDoor(Transform door)
        {
            if (_doors == null) _doors = new Dictionary<string, Transform>();
            if (!_doors.ContainsKey(door.name))
            {
                _doors.Add(door.name, door);
            }
        }
        public void AddLevelAdvanceTeleporter(Vector3 position, Vector3 targetPosition, int newLevelId)
        {
            Transform t = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.pentagram, position, Quaternion.Euler(0, 0, 0)).transform;
            t.parent = _container;
            _instantiatedObjects.Add(t);
            for(int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                if (child.tag == "Teleporter") child.tag = "LevelAdvanceTeleporter";
            }
            t.name = string.Format("LevelAdvanceTeleporter|{0}|{1}|{2}|{3}", targetPosition.x, targetPosition.y + 1, targetPosition.z, newLevelId);
        }
        public void AddMonster(GameObject monster, int idInRoom)
        {
            if (_monsters == null) _monsters = new Dictionary<int, Monster>();
            Vector3 position = FindPositionForRoomObject();
            position.y = _floorY; 

            Transform monsterT = UnityEngine.Object.Instantiate(monster, position, Quaternion.Euler(0, 0, 0)).transform;
            monsterT.tag = "Monster";
            monsterT.name = string.Format("Monster Id:{0}:{1}", id, idInRoom);
            monsterT.parent = _container;

            monsterT.gameObject.SetActive(false);
            _instantiatedObjects.Add(monsterT);
            _monsters.Add(idInRoom, new Monster(monsterT, id, idInRoom));
            _roomGrid.UpdateGrid();
        }
        public DecorationPlaceholder AddTeleporter(Teleporter t)
        {
            if (_teleporters == null) _teleporters = new List<Teleporter>();

            DecorationPlaceholder dph = (t.source == null) ?
                AddDecoration(GlobalBuildingMaterials.pentagram, 6, 4, 4, 4) :
                AddDecoration(GlobalBuildingMaterials.pentagram, (Vector3)t.source, 6, 4, 4, 4);
            t.source = dph.position;
            _teleporters.Add(t);
            return dph;
        }
        public void AddTorch(Vector3 v)
        {
            if (_torches == null) _torches = new List<Vector3>();
            _torches.Add(v);
        }
        public virtual void DestroyRoom()
        {
            if (_container != null) UnityEngine.Object.Destroy(_container.gameObject);
            foreach (Transform t in _instantiatedObjects)
            {
                if (t.gameObject != null) UnityEngine.Object.Destroy(t.gameObject);
            }
            _instantiatedObjects = new List<Transform>();
            _isDrawn = false;
            _isRendered = false;
        }
        public virtual void DrawRoom()
        {
            _container = new GameObject().transform;
            _container.name = string.Format("Room {0} container", id); // putting this here because rooms are created to see if they fit and then quickly destroyed
            if (_levelMinimapContainer == null) _roomMinimapContainer.parent = _container;

            Transform floorContainer = DrawFloor();
            floorContainer.parent = _container;

            Transform wallsContainer = DrawWalls();
            if (wallsContainer != null) wallsContainer.parent = _container;

            Transform columnsContainer = DrawColumns();
            if(columnsContainer != null) columnsContainer.parent = _container;

            Transform torchesContainer = DrawTorches();
            if (torchesContainer != null) torchesContainer.parent = _container;

            Transform decorationsContainer = DrawDecorations();
            if (decorationsContainer != null) decorationsContainer.parent = _container;

            Transform roomEntryTrigger = AddRoomEntryTrigger();
            roomEntryTrigger.parent = _container;

            _isDrawn = true;
            _isRendered = true;
            UpdateKillSheet();
            _roomGrid = new RoomGrid(id, _northEastUp, _southWestDown, 0.25f);
        }
        public void FadeTorches()
        {
            _torchesContainer.BroadcastMessage("FadeIn");
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
        public Monster GetMonster(int idInRoom)
        {
            Monster m;
            _monsters.TryGetValue(idInRoom, out m);
            return m; 
        }
        public RoomGrid GetRoomGrid() { return _roomGrid; }
        public List<Teleporter> GetTeleporters() { return _teleporters;  }

        /// <summary>
        /// this is only used for debugging
        /// </summary>
        public void DrawPathFindingGizmos()
        {
            if(_roomGrid != null)_roomGrid.DrawGizmos();
        }
        public bool IsCleared()
        {
            return _isCleared;
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
                if(thing != null) thing.gameObject.SetActive(isActive);
            }
            _isRendered = isActive;
        }
        public void UnlockDoors()
        {
            if (_doors != null)
            {
                foreach (KeyValuePair<string, Transform> doorPair in _doors)
                {
                    doorPair.Value.SendMessage("UnlockDoor");
                }
            }
        }
        public void UpdateKillSheet()
        {
            if(_monsters == null || _monsters.Count == 0)
            {
                _isCleared = true;
                return;
            }
            foreach(KeyValuePair<int, Monster> kvp in _monsters)
            {
                if(kvp.Value.IsAlive())
                {
                    _isCleared = false;
                    return;
                }
            }
            _isCleared = true;
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
            colliderTrigger.name = string.Format("Trigger collider for room ID:{0}", id);
            colliderTrigger.tag = "RoomEntryTrigger";

            triggerContainer.parent = _container;
            _instantiatedObjects.Add(triggerContainer);

            return triggerContainer;
        }
        private void AddRoomSquareOccupant(List<Vector2> allSquares)
        {
            foreach(Vector2 position in allSquares)
            {
                AddRoomSquareOccupant(position.x, position.y);
            }
        }
        private void AddRoomSquareOccupant(float x, float z)
        {
            int totalColumns = (int)(_easternEdgeX - _westernEdgeX);
            int totalRows = (int)(_northernEdgeZ - _southernEdgeZ);
            int rowNumber = (int)(x - _westernEdgeX);
            int index = (rowNumber * totalColumns) + (int)(z - _southernEdgeZ);
            //_grid[index] = true;
            
        }
        private Transform DrawDecorations()
        {
            if (_decorations == null) return null;

            Transform container = new GameObject().transform;
            container.name = string.Format("Room {0} decorations container", id);

            foreach (DecorationPlaceholder d in _decorations)
            {
                Transform t = UnityEngine.Object.Instantiate(d.gameObject, d.position, Quaternion.Euler(0, 0, 0)).transform;
                t.name = d.name;
                if (t.tag == "baseMaterial" || t.gameObject.tag == "baseMaterial") SetColor(t);
                for (int i = 0; i < t.childCount; i++)
                {
                    Transform child = t.GetChild(i);
                    if (child.tag == "baseMaterial") SetColor(child);
                }
                t.parent = container;
                t.gameObject.layer = LayerMask.NameToLayer(GlobalMapParameters.unwalkableLayerName);
                //t.gameObject.SetActive(false);
                _instantiatedObjects.Add(t);
            }
            return container;
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



            // draw the minimap lines
            GameObject gameObject = new GameObject();
            LineRenderer line = gameObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(begin.x, _floorY, begin.z));
            line.SetPosition(1, new Vector3(end.x, _floorY, end.z));
            line.startWidth = 1.0f;
            line.endWidth = 1.0f;
            line.useWorldSpace = true;
            //line.material = GlobalBuildingMaterials.miniMapLineMaterial;
            line.startColor = Color.red;
            line.endColor = Color.red;
            gameObject.layer = LayerMask.NameToLayer("Minimap");
            gameObject.transform.parent = _roomMinimapContainer;

            



            // now draw the real stuff

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

            Transform column1 = SetColumn(new Vector3(_easternEdgeX, 1 + _tileHeight + _floorY, _southernEdgeZ), Quaternion.Euler(90, 0, 0)); // SE column
            Transform column2 = SetColumn(new Vector3(_easternEdgeX, 1 + _tileHeight + _floorY, _northernEdgeZ), Quaternion.Euler(90, 0, 0)); // NE column
            Transform column3 = SetColumn(new Vector3(_westernEdgeX, 1 + _tileHeight + _floorY, _northernEdgeZ), Quaternion.Euler(90, 0, 0)); // NW column
            Transform column4 = SetColumn(new Vector3(_westernEdgeX, 1 + _tileHeight + _floorY, _southernEdgeZ), Quaternion.Euler(90, 0, 0)); // SW column
            column1.parent = container;
            column2.parent = container;
            column3.parent = container;
            column4.parent = container;
            return container;
        }
        private Transform DrawTorches()
        {
            _torchesContainer = new GameObject().transform;
            _torchesContainer.name = string.Format("Room {0} torches container", id);

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
                torch.parent = _torchesContainer;
            }

            return _torchesContainer;
        }
        private Vector3 FindPositionForRoomObject(int northPad = 0, int eastPad = 0, int southPad = 0, int westPad = 0)
        {
            if(_roomGrid == null) _roomGrid = new RoomGrid(id, _northEastUp, _southWestDown, 0.25f);


            int x = RNG.getRandomInt((int)_westernEdgeX + 1, (int)_easternEdgeX - 1);
            int z = RNG.getRandomInt((int)_southernEdgeZ + 1, (int)_northernEdgeZ - 1);

            bool isClear = true;
            List<Vector2> squaresNeeded = new List<Vector2>();
            squaresNeeded.Add(new Vector2(x, z));
            for (int i = 0; i < northPad; i++) squaresNeeded.Add(new Vector2(x, z + (i + 1)));
            for (int i = 0; i < southPad; i++) squaresNeeded.Add(new Vector2(x, z - (i + 1)));
            for (int i = 0; i < eastPad; i++) squaresNeeded.Add(new Vector2(x + (i + 1), z));
            for (int i = 0; i < westPad; i++) squaresNeeded.Add(new Vector2(x - (i + 1), z));

            foreach(Vector2 square in squaresNeeded)
            {
                if (!_roomGrid.GetNodeFromWorldPosition(new Vector3(x, _floorY, z)).isWalkable)
                {
                    isClear = false;
                    break;
                }
            }
            if (isClear)
            {
                return new Vector3(x, _floorY, z);
            }
            else return FindPositionForRoomObject(northPad, eastPad, southPad, westPad);
        }
        //private void InitializeGrid()
        //{
        //    int totalColumns = (int)(_easternEdgeX - _westernEdgeX) + 1;
        //    int totalRows = (int)(_northernEdgeZ - _southernEdgeZ) + 1;
        //    int numSquares = totalColumns * totalRows;
        //    _grid = new bool[numSquares];
        //}
        //private bool isRoomSquareOccupied(float x, float z)
        //{
        //    return !_roomGrid.GetNodeFromWorldPosition(new Vector3(x, _floorY, z)).isWalkable;
        //    //int totalColumns = (int)(_easternEdgeX - _westernEdgeX) +1;
        //    //int totalRows = (int)(_northernEdgeZ - _southernEdgeZ) +1;
        //    //int rowNumber = (int)(z - _southernEdgeZ);
        //    //int index = (rowNumber * totalColumns) + (int)(x - _westernEdgeX);
        //    ////return _grid[index];
        //}
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
            thisBrick.gameObject.layer = LayerMask.NameToLayer(GlobalMapParameters.unwalkableLayerName);
            return thisBrick;
        }
        private Transform SetColumn(Vector3 position, Quaternion rotation)
        {
            Transform thisColumn = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.column001, position, rotation).AddComponent<BoxCollider>().transform;
            _instantiatedObjects.Add(thisColumn);
            SetColor(thisColumn);
            thisColumn.gameObject.layer = LayerMask.NameToLayer(GlobalMapParameters.unwalkableLayerName);
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