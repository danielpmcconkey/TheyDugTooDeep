#define DRAW_MAP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;

namespace GameLibrary.Map
{
    public class Level
    {
        public int levelNumber;
        public Dictionary<int, Room> rooms {get { return _rooms;  } }
        private Dictionary<int, Room> _rooms;
        private Vector3 _startingPoint;
        private MapSquare[] _grid;
        private int _numMainRooms;
        private Color _baseColor;
        private Transform _container;
        private Transform _miniMapContainer;

        
        public Level(Vector3 startingPoint, int num, Color? baseColor = null)
        {
            levelNumber = num;
            InitializeGrid();
            _rooms = new Dictionary<int, Room>();
            _startingPoint = startingPoint;
            _numMainRooms = RNG.getRandomInt(GlobalMapParameters.minRoomsPerFloor, GlobalMapParameters.maxRoomsPerFloor);

            float r = (float)RNG.getRandomInt(0, 100) / 100;
            float g = (float)RNG.getRandomInt(0, 100) / 100;
            float b = (float)RNG.getRandomInt(0, 100) / 100;
            _baseColor = (baseColor == null) ? new Color(r, g, b) : (Color)baseColor;

            _container = new GameObject().transform;
            _container.name = string.Format("Level {0} container", levelNumber);
            _miniMapContainer = new GameObject().transform;
            _miniMapContainer.name = string.Format("Level {0} minimap container", levelNumber);

        }

        #region // public interface methods
        public void CreateStarterRoom(int tileSet)
        {
            // this is split out because the portal level only needs a starter room
            // starter room
            // starter room is always startingRoomRadius meters in each direction around the starting point
            float northEdge = _startingPoint.z + GlobalMapParameters.startingRoomRadius;
            float southEdge = _startingPoint.z - GlobalMapParameters.startingRoomRadius;
            float eastEdge = _startingPoint.x + GlobalMapParameters.startingRoomRadius;
            float westEdge = _startingPoint.x - GlobalMapParameters.startingRoomRadius;


            Room starterRoom = new Room(0, new Vector3(westEdge, _startingPoint.y, southEdge)
                , new Vector3(eastEdge, GlobalMapParameters.roomHeight, northEdge), tileSet, _baseColor, _miniMapContainer);
            PopulateGridSquares(starterRoom);
            _rooms.Add(0, starterRoom);
        }


        public void CreateRooms()
        {
            int tileSet = 1;
            CreateStarterRoom(tileSet);
            

            // add the starting pentagram before you create other rooms so that the closed-loop teleporters don't collide
            Teleporter t = new Teleporter()
            {
                sourceRoom = rooms[0],
                source = new Vector3(0, 0, 0),
                destinationRoom = null
            };
            DecorationPlaceholder dph = rooms[0].AddTeleporter(t);
            dph.name = string.Empty;


            for (int i = 1; i < _numMainRooms; i++) // added 1 room already
            {
                Room room = CreateRoom(i, tileSet);
                _rooms.Add(i, room);
            }
            CreateCorridors(tileSet);
            CreateRightAngleConnections(tileSet);
#if (DEBUG && DRAW_MAP)
            DrawMapToTextFile();
#endif
            CreateTeleportersForClosedLoops();
            Transform doorsContainer = AddDoors();
            doorsContainer.parent = _container;
            doorsContainer.gameObject.SetActive(false); // this is done because doors aren't rendered as needed, so you want to prevent doors from other levels from displaying
        }
#if (DEBUG && DRAW_MAP)
        private void DrawMapToTextFile()
        {
            char[,] roomsIdsGrid = new char[GlobalMapParameters.mapSize * 2, GlobalMapParameters.mapSize * 2];
            for(int i = 0; i < GlobalMapParameters.mapSize *2; i++)
            {
                for(int j = 0; j < GlobalMapParameters.mapSize * 2; j ++)
                {
                    roomsIdsGrid[i, j] = ' ';
                }
            }
            foreach(KeyValuePair<int, Room> kvp in _rooms)
            {
                int id = kvp.Key;
                Room r = kvp.Value;
                // north and south walls
                for (int i = (int)r.GetEdge(Direction.WEST) + GlobalMapParameters.mapSize; i <= (int)r.GetEdge(Direction.EAST) + GlobalMapParameters.mapSize; i++)
                {
                    roomsIdsGrid[i, (int)r.GetEdge(Direction.NORTH) + GlobalMapParameters.mapSize] = '-';
                    roomsIdsGrid[i, (int)r.GetEdge(Direction.SOUTH) + GlobalMapParameters.mapSize] = '-';
                }
                // east and west walls
                for (int i = (int)r.GetEdge(Direction.SOUTH) + GlobalMapParameters.mapSize; i <= (int)r.GetEdge(Direction.NORTH) + GlobalMapParameters.mapSize; i++)
                {
                    roomsIdsGrid[(int)r.GetEdge(Direction.EAST) + GlobalMapParameters.mapSize, i] = '|';
                    roomsIdsGrid[(int)r.GetEdge(Direction.WEST) + GlobalMapParameters.mapSize, i] = '|';
                }
                // put IDs in center
                int width = Mathf.RoundToInt(r.GetEdge(Direction.EAST) - r.GetEdge(Direction.WEST));
                int depth = Mathf.RoundToInt(r.GetEdge(Direction.NORTH) - r.GetEdge(Direction.SOUTH));
                int centerX = Mathf.RoundToInt(r.GetEdge(Direction.WEST) + (width / 2)) + GlobalMapParameters.mapSize;
                int centerY = Mathf.RoundToInt(r.GetEdge(Direction.SOUTH) + (depth / 2)) + GlobalMapParameters.mapSize;
                char[] idToChars = id.ToString().ToCharArray();
                if (id < 10) roomsIdsGrid[centerX, centerY] = idToChars[0];
                else
                {
                    roomsIdsGrid[centerX, centerY] = idToChars[0];
                    roomsIdsGrid[centerX + 1, centerY] = idToChars[1];
                }
            }
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("E:\\test.txt"))
            {
                for(int y = (GlobalMapParameters.mapSize * 2) - 1; y >= 0; y--)    // go down to up because lower numbers in unity are at the "bottom"
                {
                    for(int x = 0; x < GlobalMapParameters.mapSize * 2; x++)
                    {
                        sw.Write(roomsIdsGrid[x, y]);
                    }
                    sw.Write(System.Environment.NewLine);
                }
            }
        }
#endif
        public void DecorateRooms()
        {
            foreach(KeyValuePair<int, Room> entry in _rooms) AddTorchesToRoom(entry.Value);
        }
        public void RenderMinimap(bool isActive)
        {
            _miniMapContainer.gameObject.SetActive(isActive);
        }
        public void PopulateRooms()
        {
            for(int i = 0; i < _numMainRooms; i++)
            {
                Room r = _rooms[i];
                float width = r.GetEdge(Direction.EAST) - r.GetEdge(Direction.WEST);
                float depth = r.GetEdge(Direction.NORTH) - r.GetEdge(Direction.SOUTH);
                float area = width * depth;
                int maxMonsters = (int)Mathf.Floor(area / 20);
                int minMonsters = (int)Mathf.Floor(area / 40);
                int numMonsters = RNG.getRandomInt(minMonsters, maxMonsters);
                if (!r.IsDrawn()) r.DrawRoom(); // do this to create the grid so we can detect object collisions
                for (int j = 0; j < numMonsters; j++)
                {
                    r.AddMonster(GlobalBuildingMaterials.MonsterToken, j);
                }
            }
        }
        public void RenderDoors(bool isActive)
        {
            Transform doorsContainer = _container.Find(string.Format("Level {0} doors container", levelNumber));
            if(doorsContainer!= null) doorsContainer.gameObject.SetActive(isActive);
        }
        public void RenderEntireLevel(bool isActive)
        {
            RenderDoors(isActive);
            for (int i = 0; i < _rooms.Count; i++)
            {
                if (!_rooms[i].IsDrawn())
                {
                    _rooms[i].DrawRoom(); // always make sure the room is drawn, even if you just want to deactive it
                    _rooms[i].GetContainer().parent = _container; //parent room to _container
                }
                _rooms[i].RenderRoomObjects(isActive);
            }

        }
        #endregion // public methods


        #region room creation methods
        private Transform AddDoors()
        {
            Transform container = new GameObject().transform;
            container.name = string.Format("Level {0} doors container", levelNumber);

            List<RoomConnection> connections = new List<RoomConnection>();
            foreach(KeyValuePair<int, Room> roomPair in rooms)
            {
                Room r1 = roomPair.Value;
                int r1Id = roomPair.Key;
                foreach(RoomAdjacency ra in r1.GetAdjacencies())
                {
                    Room r2 = ra.room;
                    int r2Id = r2.id;
                    RoomConnection rc = new RoomConnection()
                    {
                        starterRoom = (r1Id < r2Id) ? r1 : r2,
                        connectedRoom = (r1Id > r2Id) ? r1 : r2
                    };
                    if(!connections.Contains(rc))
                    {
                        //Debug.Log(string.Format("New door between rooms {0} and {1}", rc.starterRoom.id, rc.connectedRoom.id));
                        connections.Add(rc);

                        Quaternion rotation = Quaternion.Euler(0, 0, 0);
                        Vector3 position = new Vector3(0, 0, 0);
                        switch (ra.adjacentWall)
                        {
                            case Direction.EAST:
                                rotation = Quaternion.Euler(0, 90, 0);
                                float x = r1.GetEdge(Direction.EAST);
                                float r2Width = r2.GetEdge(Direction.NORTH) - r2.GetEdge(Direction.SOUTH);
                                float z = r2.GetEdge(Direction.NORTH) - (r2Width / 2);
                                position = new Vector3(x, _startingPoint.y, z);
                                break;
                            case Direction.WEST:
                                rotation = Quaternion.Euler(0, 90, 0);
                                x = r1.GetEdge(Direction.WEST);
                                r2Width = r2.GetEdge(Direction.NORTH) - r2.GetEdge(Direction.SOUTH);
                                z = r2.GetEdge(Direction.NORTH) - (r2Width / 2);
                                position = new Vector3(x, _startingPoint.y, z);
                                break;
                            case Direction.NORTH:
                                rotation = Quaternion.Euler(0, 0, 0);
                                z = r1.GetEdge(Direction.NORTH);
                                r2Width = r2.GetEdge(Direction.EAST) - r2.GetEdge(Direction.WEST);
                                x = r2.GetEdge(Direction.EAST) - (r2Width / 2);
                                position = new Vector3(x, _startingPoint.y, z);
                                break;
                            case Direction.SOUTH:
                                rotation = Quaternion.Euler(0, 0, 0);
                                z = r1.GetEdge(Direction.SOUTH);
                                r2Width = r2.GetEdge(Direction.EAST) - r2.GetEdge(Direction.WEST);
                                x = r2.GetEdge(Direction.EAST) - (r2Width / 2);
                                position = new Vector3(x, _startingPoint.y, z);
                                break;
                        }





                        GameObject doorObj = UnityEngine.Object.Instantiate(GlobalBuildingMaterials.door001, position, rotation);
                        Transform door = doorObj.transform;
                        //door.gameObject.SetActive(false); // initialize doors to be invisible
                        door.name = string.Format("DoorBetween:{0}:{1}", rc.starterRoom.id, rc.connectedRoom.id);
                        rooms[rc.starterRoom.id].AddDoor(door);
                        rooms[rc.connectedRoom.id].AddDoor(door);


                        float rModifier = (float)RNG.getRandomInt(0, GlobalMapParameters.colorVarianceMax) / 100;
                        float gModifier = (float)RNG.getRandomInt(0, GlobalMapParameters.colorVarianceMax) / 100;
                        float bModifier = (float)RNG.getRandomInt(0, GlobalMapParameters.colorVarianceMax) / 100;

                        door.Find("door001_frame").GetComponent<Renderer>().material.color = new Color(_baseColor.r + rModifier, _baseColor.g + gModifier, _baseColor.b + bModifier);

                        door.parent = container;
                    }
                }
            }
            return container;
        }
        private void AddTorchesToRoom(Room r)
        {
            AddTorchesToWall(r, Direction.NORTH);
            AddTorchesToWall(r, Direction.SOUTH);
            AddTorchesToWall(r, Direction.EAST);
            AddTorchesToWall(r, Direction.WEST);
        }
        private void AddTorchesToWall(Room r, Direction direction)
        {
            float torchHeight = 1.25f;
            float torchWallOffset = (direction == Direction.SOUTH || direction == Direction.WEST) 
                ? GlobalMapParameters.torchWallOffset : 0 - GlobalMapParameters.torchWallOffset;
            DirectionSet ds = DirectionHelper.GetDirectionSetFromPrimary(direction);

            List<float> wallPoints = new List<float>();
            wallPoints.Add(r.GetEdge(ds.PerpendicularMin));
            List<RoomAdjacency> adjacentRooms = r.GetAdjacencies();
            for (int i = 0; i < adjacentRooms.Count; i++)
            {
                if (adjacentRooms[i].adjacentWall == ds.Primary)
                {
                    float otherRoomPerpMin = adjacentRooms[i].room.GetEdge(ds.PerpendicularMin);
                    float otherRoomPerpMax = adjacentRooms[i].room.GetEdge(ds.PerpendicularMax);
                    if(otherRoomPerpMin >= r.GetEdge(ds.PerpendicularMin) && !wallPoints.Contains(otherRoomPerpMin)) wallPoints.Add(otherRoomPerpMin); // only add a torch if within the bounds of r
                    if (otherRoomPerpMax <= r.GetEdge(ds.PerpendicularMax) && !wallPoints.Contains(otherRoomPerpMax)) wallPoints.Add(otherRoomPerpMax);
                }
            }
            if(!wallPoints.Contains(r.GetEdge(ds.PerpendicularMax)))wallPoints.Add(r.GetEdge(ds.PerpendicularMax));
            if (wallPoints.Count > 2) wallPoints.Sort(); // prevents going out of order
            for (int j = 0; j < wallPoints.Count - 1; j ++) // take the wall points in pairs
            {
                
                float lengthOfSegment = wallPoints[j + 1] - wallPoints[j];
               
                if (lengthOfSegment > GlobalMapParameters.corridorWidth)
                {
                    float x = 0;
                    float z = 0;
                    if (direction == Direction.EAST || direction == Direction.WEST)
                    {
                        x = r.GetEdge(ds.Primary) + torchWallOffset;
                        z = wallPoints[j] + (lengthOfSegment / 2);
                    }
                    else
                    {
                        x = wallPoints[j] + ((wallPoints[j + 1] - wallPoints[j]) / 2);
                        z = r.GetEdge(ds.Primary) + torchWallOffset;
                    }

                    r.AddTorch(new Vector3(x, torchHeight, z));
                }
                
            }
        }
        private void CreateCorridors(int tileSet)
        {
            for (int i = 0; i < _numMainRooms; i++) // can't foreach the list because you add to it while you're doing it
            {
                if (_rooms[i].GetAdjacencies().Count == 0)
                {
                    RoomConnection connection = FindRoomToConnect(_rooms[i], tileSet);
                    if (connection != null)
                    {
                        int id = _rooms.Count;
                        connection.corridor.id = id;
                        _rooms.Add(id, connection.corridor);
                        connection.starterRoom.AddAdjacency(new RoomAdjacency()
                        {
                            room = connection.corridor,
                            adjacentWall = connection.corridor.direction
                        });
                        connection.starterRoom.AddConnection(connection.connectedRoom);
                        connection.connectedRoom.AddAdjacency(new RoomAdjacency()
                        {
                            room = connection.corridor,
                            adjacentWall = DirectionHelper.GetOppositeDirection(connection.corridor.direction)
                        });
                        connection.corridor.AddAdjacency(new RoomAdjacency()
                        {
                            room = connection.starterRoom,
                            adjacentWall = DirectionHelper.GetOppositeDirection(connection.corridor.direction)
                        });
                        connection.corridor.AddAdjacency(new RoomAdjacency()
                        {
                            room = connection.connectedRoom,
                            adjacentWall = connection.corridor.direction
                        });


                        connection.connectedRoom.AddConnection(connection.starterRoom);
                    }
                }
            }
            // try for a second connection where possible
            for (int i = 0; i < _numMainRooms; i++) // can't foreach the list because you add to it while you're doing it
            {
                RoomConnection connection = FindRoomToConnect(_rooms[i], tileSet);
                if (connection != null)
                {
                    int id = _rooms.Count;
                    connection.corridor.id = id;
                    _rooms.Add(id, connection.corridor);
                    connection.starterRoom.AddAdjacency(new RoomAdjacency() { room = connection.corridor, adjacentWall = connection.corridor.direction });
                    connection.connectedRoom.AddAdjacency(new RoomAdjacency()
                    {
                        room = connection.corridor,
                        adjacentWall = DirectionHelper.GetOppositeDirection(connection.corridor.direction)
                    });
                    connection.corridor.AddAdjacency(new RoomAdjacency()
                    {
                        room = connection.starterRoom,
                        adjacentWall = DirectionHelper.GetOppositeDirection(connection.corridor.direction)
                    });
                    connection.corridor.AddAdjacency(new RoomAdjacency()
                    {
                        room = connection.connectedRoom,
                        adjacentWall = connection.corridor.direction
                    });
                    connection.starterRoom.AddConnection(connection.connectedRoom);
                    connection.connectedRoom.AddConnection(connection.starterRoom);
                }
            }
        }
        private void CreateRightAngleConnections(int tileSet)
        {
            for(int i = 0; i < _numMainRooms; i++)
            {
                Room source = _rooms[i];
                // iterate through all other rooms to see if there's a fit
                for (int j = 0; j < _numMainRooms; j++)
                {
                    if (j != i)
                    {
                        Room target = _rooms[j];
                        float sourceWidth = source.GetEdge(Direction.EAST) - source.GetEdge(Direction.WEST);
                        float sourceDepth = source.GetEdge(Direction.NORTH) - source.GetEdge(Direction.SOUTH);
                        float sourceX = source.GetEdge(Direction.WEST) + (sourceWidth / 2);
                        float sourceZ = source.GetEdge(Direction.SOUTH) + (sourceDepth / 2);

                        float targetWidth = target.GetEdge(Direction.EAST) - target.GetEdge(Direction.WEST);
                        float targetDepth = target.GetEdge(Direction.NORTH) - target.GetEdge(Direction.SOUTH);
                        float targetX = target.GetEdge(Direction.WEST) + (targetWidth / 2);
                        float targetZ = target.GetEdge(Direction.SOUTH) + (targetDepth / 2);

                        // rooms have to be far enough apart in both planes, first
                        bool roomsMightWork = true;
                        if (sourceX == targetX || sourceZ == targetZ) roomsMightWork = false;
                        if (roomsMightWork && sourceX > targetX) // source is more east than target
                        {
                            if ((source.GetEdge(Direction.WEST) - targetX - // the lengeth of the X leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                <= GlobalMapParameters.minCorridorLength)
                                roomsMightWork = false; // not enough X room
                            if ((source.GetEdge(Direction.WEST) - targetX - // the lengeth of the X leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                >= GlobalMapParameters.maxCorridorLength)
                                roomsMightWork = false; // too much X room
                        }
                        if (roomsMightWork && sourceX < targetX) // source is more west than target
                        {
                            if ((target.GetEdge(Direction.WEST) - sourceX - // the lengeth of the X leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                <= GlobalMapParameters.minCorridorLength)
                                roomsMightWork = false; // not enough X room
                            if ((target.GetEdge(Direction.WEST) - sourceX - // the lengeth of the X leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                >= GlobalMapParameters.maxCorridorLength)
                                roomsMightWork = false; // too much X room
                        }
                        if (roomsMightWork && sourceZ > targetZ) // source is more north than target
                        {
                            if ((source.GetEdge(Direction.SOUTH) - targetZ - // the lengeth of the Z leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                <= GlobalMapParameters.minCorridorLength)
                                roomsMightWork = false; // not enough Z room
                            if ((source.GetEdge(Direction.SOUTH) - targetZ - // the lengeth of the Z leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                >= GlobalMapParameters.maxCorridorLength)
                                roomsMightWork = false; // too much z room
                        }
                        if (roomsMightWork && sourceZ < targetZ) // source is more south than target
                        {
                            if ((target.GetEdge(Direction.SOUTH) - sourceZ - // the lengeth of the Z leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                <= GlobalMapParameters.minCorridorLength)
                                roomsMightWork = false; // not enough Z room
                            if ((target.GetEdge(Direction.SOUTH) - sourceZ - // the lengeth of the Z leg
                                (GlobalMapParameters.rightAngleConnectorSize / 2)) // half of the center hub "room"
                                >= GlobalMapParameters.maxCorridorLength)
                                roomsMightWork = false; // too much z room
                        }

                        if (roomsMightWork)
                        {
                            /****************************************************************************************
                             *  there's room (maybe). now create two corridors with a tiny room in the middle. there 
                             *  are 2 ways to go here: sourceX <--> targetZ or sourceZ <--> targetX. but they're the 
                             *  same if you try both ways (room 1 is source and 2 is target and then room 2 is source 
                             *  and room 1 is target). so only try sourceX <--> targetZ
                             *  *************************************************************************************/

                            // centerpoint room
                            float cpCenterX = sourceX;
                            float cpCenterZ = targetZ;
                            float cpNorthEdge = cpCenterZ + (GlobalMapParameters.rightAngleConnectorSize / 2);
                            float cpSouthEdge = cpCenterZ - (GlobalMapParameters.rightAngleConnectorSize / 2);
                            float cpEastEdge = cpCenterX + (GlobalMapParameters.rightAngleConnectorSize / 2);
                            float cpWestEdge = cpCenterX - (GlobalMapParameters.rightAngleConnectorSize / 2);
                            Room centerPoint = new Room(
                                                -1,
                                                new Vector3(cpWestEdge, 0, cpSouthEdge),
                                                new Vector3(cpEastEdge, GlobalMapParameters.roomHeight, cpNorthEdge),
                                                tileSet,
                                                _baseColor,
                                                _miniMapContainer
                                                );
                            if (IsThereSpaceForTheRoom(centerPoint, true))
                            {
                                // draw the Z leg
                                float zLegNorthEdge = (sourceZ > targetZ) ? source.GetEdge(Direction.SOUTH) : centerPoint.GetEdge(Direction.SOUTH);
                                float zLegSouthEdge = (sourceZ > targetZ) ? centerPoint.GetEdge(Direction.NORTH) : source.GetEdge(Direction.NORTH);
                                float zLegEastEdge = cpCenterX + (GlobalMapParameters.corridorWidth / 2);
                                float zLegWestEdge = cpCenterX - (GlobalMapParameters.corridorWidth / 2);
                                Corridor zLeg = new Corridor(
                                                -1,
                                                new Vector3(zLegWestEdge, 0, zLegSouthEdge),
                                                new Vector3(zLegEastEdge, GlobalMapParameters.roomHeight, zLegNorthEdge),
                                                tileSet,
                                                _baseColor,
                                                _miniMapContainer,
                                                Direction.NORTH
                                                );
                                if (IsThereSpaceForTheRoom(zLeg))
                                {
                                    // draw the X leg
                                    float xLegEastEdge = (sourceX > targetX) ? centerPoint.GetEdge(Direction.WEST) : target.GetEdge(Direction.WEST);
                                    float xLegWestEdge = (sourceX > targetX) ? target.GetEdge(Direction.EAST) : centerPoint.GetEdge(Direction.EAST);
                                    float xLegNorthEdge = cpCenterZ + (GlobalMapParameters.corridorWidth / 2);
                                    float xLegSouthEdge = cpCenterZ - (GlobalMapParameters.corridorWidth / 2);
                                    Corridor xLeg = new Corridor(
                                                    -1,
                                                    new Vector3(xLegWestEdge, 0, xLegSouthEdge),
                                                    new Vector3(xLegEastEdge, GlobalMapParameters.roomHeight, xLegNorthEdge),
                                                    tileSet,
                                                    _baseColor,
                                                    _miniMapContainer,
                                                    Direction.EAST
                                                    );
                                    if (IsThereSpaceForTheRoom(xLeg))
                                    {
                                        // we have a match
                                        PopulateGridSquares(centerPoint);
                                        PopulateGridSquares(zLeg);
                                        PopulateGridSquares(xLeg);

                                        // room adjacencies
                                        centerPoint.AddAdjacency(
                                            new RoomAdjacency() { room = zLeg, adjacentWall = (sourceZ > targetZ) ? Direction.NORTH : Direction.SOUTH }
                                            );
                                        centerPoint.AddAdjacency(
                                            new RoomAdjacency() { room = xLeg, adjacentWall = (sourceX > targetX) ? Direction.WEST : Direction.EAST }
                                            );

                                        source.AddAdjacency(
                                            new RoomAdjacency() { room = zLeg, adjacentWall = (sourceZ > targetZ) ? Direction.SOUTH : Direction.NORTH }
                                            );
                                        target.AddAdjacency(
                                            new RoomAdjacency() { room = xLeg, adjacentWall = (sourceX > targetX) ? Direction.EAST : Direction.WEST }
                                            );


                                        zLeg.AddAdjacency(
                                            new RoomAdjacency() { room = centerPoint, adjacentWall = (sourceZ > targetZ) ? Direction.SOUTH : Direction.NORTH }
                                            );
                                        xLeg.AddAdjacency(
                                            new RoomAdjacency() { room = centerPoint, adjacentWall = (sourceX > targetX) ? Direction.EAST : Direction.WEST }
                                            );
                                        zLeg.AddAdjacency(
                                            new RoomAdjacency() { room = source, adjacentWall = (sourceZ > targetZ) ? Direction.NORTH : Direction.SOUTH }
                                            );
                                        xLeg.AddAdjacency(
                                            new RoomAdjacency() { room = target, adjacentWall = (sourceX > targetX) ? Direction.WEST : Direction.EAST }
                                            );




                                        centerPoint.id = _rooms.Count;
                                        _rooms.Add(_rooms.Count, centerPoint);
                                        zLeg.id = _rooms.Count;
                                        _rooms.Add(_rooms.Count, zLeg);
                                        xLeg.id = _rooms.Count;
                                        _rooms.Add(_rooms.Count, xLeg);
                                        // add the room connections
                                        source.AddConnection(target);
                                        target.AddConnection(source);

                                    } // end IsThereSpaceForTheRoom(xLeg)
                                } // end IsThereSpaceForTheRoom(zLeg)
                            } // end IsThereSpaceForTheRoom(centerPoint)
                        } // end rooms might work

                    } // end if j != i
                } // end for j loop
                
                    
            } // end for i loop
        }
        private Room CreateRoom(int id, int tileSet)
        {
            Room room = null;
            bool roomPlaced = false;

            while (!roomPlaced)
            {
                int roomWidth = RNG.getRandomInt(GlobalMapParameters.minRoomSize, GlobalMapParameters.maxRoomSize); // distance along x axis
                int roomDepth = RNG.getRandomInt(GlobalMapParameters.minRoomSize, GlobalMapParameters.maxRoomSize); // distance along z axis


                float southEdge = RNG.getRandomInt(0 - GlobalMapParameters.mapSize, GlobalMapParameters.mapSize - roomDepth);
                float westEdge = RNG.getRandomInt(0 - GlobalMapParameters.mapSize, GlobalMapParameters.mapSize - roomWidth);
                float northEdge = southEdge + roomDepth;
                float eastEdge = westEdge + roomWidth;

                room = new Room(
                    id,
                    new Vector3(westEdge, _startingPoint.y, southEdge),
                    new Vector3(eastEdge, GlobalMapParameters.roomHeight, northEdge),
                    tileSet,
                    _baseColor,
                    _miniMapContainer
                    );

                if (IsThereSpaceForTheRoom(room, true)) roomPlaced = true;
                else
                {
                    room.DestroyRoom();
                    room = null;
                }
            }


            PopulateGridSquares(room);
            return room;

        }
        private void CreateTeleportersForClosedLoops()
        {
            /*********************************************************
             * a closed loop is a group of rooms (or just a room)
             * that are connected to each other, but not the rest of
             * the level.
             * ******************************************************/
            List<List<int>> closedLoops = GetClosedLoops();
            /*********************************************************
             * each closed loop needs to connect to one of the others
             * so cycle through the loops and have each connect
             * to the next in the list. Have the last connect to the
             * first in the list. shazam? shaZAM!
             * ******************************************************/
            if (closedLoops != null)
            {
                for (int i = 0; i < closedLoops.Count; i+=2)    // because you create both teleporters below, try to work in pairs 
                {
                    List<int> sourceLoop = closedLoops[i];
                    List<int> destinationLoop = closedLoops[(i < closedLoops.Count - 1) ? i + 1 : 0]; // if you have an odd number of loops, take the last back to the first
                    Room r1 = _rooms[sourceLoop[sourceLoop.Count - 1]]; // last room in source loop
                    Room r2 = _rooms[destinationLoop[0]]; // first room in destination loop

                    Teleporter t1 = new Teleporter() { sourceRoom = r1, source = null, destinationRoom = r2, destination = null };
                    Teleporter t2 = new Teleporter() { sourceRoom = r2, source = null, destinationRoom = r1, destination = null };
                    DecorationPlaceholder dph1 = r1.AddTeleporter(t1);
                    DecorationPlaceholder dph2 = r2.AddTeleporter(t2);
                    dph1.name = string.Format("Teleporter|{0}|{1}|{2}", dph2.position.x, dph2.position.y + 1, dph2.position.z);
                    dph2.name = string.Format("Teleporter|{0}|{1}|{2}", dph1.position.x, dph1.position.y + 1, dph1.position.z);

                    //t1.source = dph1.position;
                    //t1.destination = t2Location;
                    //t2.source = t2Location;
                    //t2.destination = t1Location;
                }
            }
        }
        private RoomConnection FindRoomToConnect(Room room, int tileSet)
        {
            bool connectionFound = false;
            int numTries = 0;

            Corridor corridor = null;
            Room connectedRoom = null;

            // pick a direction to start looking
            Direction direction = (Direction)RNG.getRandomInt(0, 3);

            while (!connectionFound && numTries < GlobalMapParameters.maxRetries)
            {
                if (numTries > 0) direction = (Direction)((int)(direction + 1) % 4); // try a different direction if the last didn't work

                // first need to get your directions to use so this can avoid a giant stupid switch on directions
                DirectionSet ds = DirectionHelper.GetDirectionSetFromPrimary(direction);

                // now try each room to see if it can fit.
                foreach (KeyValuePair<int, Room> roomEntry in _rooms)
                {
                    Room candidate = roomEntry.Value;
                    bool alreadyConnected = false;
                    if (room.GetConnections() != null)
                    {
                        foreach (Room alreadyConnectedRoom in room.GetConnections())
                            if (roomEntry.Key == alreadyConnectedRoom.id) alreadyConnected = true;
                    }
                    if (roomEntry.Key != room.id && !alreadyConnected) // it's cheating to match on itself or a room already connected
                    {
                        if (candidate.GetType().ToString() == "GameLibrary.Map.Room") // don't want to match on other corridors
                        {
                            /*
                             * what's our buy box?
                             * room has to have overlap of any 2 contiguous map square along the primary axis
                             * and be within corridor max
                             *  
                             * target room must have a western edge that is smaller than our eastern edge - 1
                             * and it must have an eastern that is greater than our western edge + 1
                             * and target's southern edge must be greater than our northern edge by corridorMin
                             * and target's souther edge cannot be greater than our norther edge + corridorMax
                             * */

                            if (candidate.GetEdge(ds.PerpendicularMin) < room.GetEdge(ds.PerpendicularMax) - 1)
                                if (candidate.GetEdge(ds.PerpendicularMax) > room.GetEdge(ds.PerpendicularMin) + 1)
                                    if (candidate.GetEdge(ds.PrimaryOpposite) >= room.GetEdge(ds.Primary) + GlobalMapParameters.minCorridorLength)
                                        if (candidate.GetEdge(ds.PrimaryOpposite) <= room.GetEdge(ds.Primary) + GlobalMapParameters.maxCorridorLength)
                                        {
                                            /*
                                             * we have a match, as long as nothing else is in the way
                                             * find the east/west bounds of the corridor
                                             * how much overlap is there?
                                             * */

                                            float corridorMinX;
                                            float corridorMaxX;

                                            corridorMinX = (candidate.GetEdge(ds.PerpendicularMin) <= room.GetEdge(ds.PerpendicularMin)) ?
                                                room.GetEdge(ds.PerpendicularMin) :
                                                candidate.GetEdge(ds.PerpendicularMin);
                                            corridorMaxX = (candidate.GetEdge(ds.PerpendicularMax) >= room.GetEdge(ds.PerpendicularMax)) ?
                                                room.GetEdge(ds.PerpendicularMax) :
                                                candidate.GetEdge(ds.PerpendicularMax);

                                            float corridorLesserEdge = RNG.getRandomInt((int)corridorMinX, (int)corridorMaxX - GlobalMapParameters.corridorWidth);
                                            float corridorGreaterEdge = corridorLesserEdge + GlobalMapParameters.corridorWidth;
                                            float corridorTargetEdge = candidate.GetEdge(ds.PrimaryOpposite);
                                            float corridorSourceEdge = room.GetEdge(ds.Primary);

                                            float northEdge = 0;
                                            float southEdge = 0;
                                            float eastEdge = 0;
                                            float westEdge = 0;

                                            if (ds.Primary == Direction.NORTH || ds.Primary == Direction.SOUTH)
                                            {
                                                northEdge = (corridorTargetEdge > corridorSourceEdge) ? corridorTargetEdge : corridorSourceEdge;
                                                southEdge = (corridorSourceEdge > corridorTargetEdge ) ? corridorTargetEdge : corridorSourceEdge;
                                                eastEdge = corridorGreaterEdge;
                                                westEdge = corridorLesserEdge;
                                            }
                                            else
                                            {
                                                eastEdge = (corridorTargetEdge > corridorSourceEdge) ? corridorTargetEdge : corridorSourceEdge;
                                                westEdge = (corridorSourceEdge > corridorTargetEdge) ? corridorTargetEdge : corridorSourceEdge;
                                                northEdge = corridorGreaterEdge;
                                                southEdge = corridorLesserEdge;
                                            }
                                            Corridor corridorCandidate = new Corridor(
                                                -1,
                                                new Vector3(westEdge, _startingPoint.y, southEdge),
                                                new Vector3(eastEdge, GlobalMapParameters.roomHeight, northEdge),
                                                tileSet,
                                                _baseColor,
                                                _miniMapContainer,
                                                ds.Primary
                                                );

                                            if (IsThereSpaceForTheRoom(corridorCandidate))
                                            {
                                                connectionFound = true;
                                                corridor = corridorCandidate;
                                                connectedRoom = candidate;
                                            }
                                            break;
                                            
                                        }
                        } // end long thing
                    }
                } // end cycling through room candidates


                numTries++;
            } // end while loop
            if (corridor != null && connectedRoom != null)
            {
                RoomConnection connection = new RoomConnection();
                connection.connectedRoom = connectedRoom;
                connection.starterRoom = room;
                connection.corridor = corridor;
                PopulateGridSquares(connection.corridor);
                return connection;
            }
            return null;
        }
        private List<List<int>> GetClosedLoops()
        {
            List<List<int>> closedLoops = new List<List<int>>();
            for (int i = 0; i < _numMainRooms; i++)
            {
                List<int> connectedRoomList = new List<int>();
                connectedRoomList.Add(_rooms[i].id);
                connectedRoomList = GetConnectedRoomIdsForRoom(_rooms[i], connectedRoomList);
                if (connectedRoomList.Count == _numMainRooms) // we have no closed loops
                    return null;
                /* ******************************************
                 * before adding, make sure that this closed
                 * loop doesn't already exist. A loop of 
                 * 5,2,4 is the same as a loop of 2,4,5.
                 * ******************************************/
                connectedRoomList.Sort();
                bool duplicate = false;
                if (closedLoops.Count > 0) // if it *is* the first loop, go ahead and skip all logic and add
                {
                    foreach (List<int> loop in closedLoops)
                    {
                        if (loop.Count == connectedRoomList.Count) // if the count *is* different, you can skip the check
                        {
                            bool thisLoopMatches = true;
                            for (int j = 0; j < loop.Count; j++)
                            {
                                if (loop[j] != connectedRoomList[j]) // this is not the same loop
                                {
                                    thisLoopMatches = false;
                                    break;
                                }
                            }
                            if (thisLoopMatches) duplicate = true;
                        }
                    }
                }
                if (!duplicate) closedLoops.Add(connectedRoomList);
            }
            return closedLoops;
        }
        private List<int> GetConnectedRoomIdsForRoom(Room r, List<int> connectedRoomList)
        {
            /********************************************************************
             * This is a recursive function to get a list of all the rooms 
             * a room is connected to, all the rooms those rooms are
             * connected to, etc.
             * *****************************************************************/
            foreach (Room r2 in r.GetConnections())
            {
                if (!connectedRoomList.Contains(r2.id))
                {
                    connectedRoomList.Add(r2.id);
                    List<int> childList = GetConnectedRoomIdsForRoom(r2, connectedRoomList);
                    foreach (int i in childList)
                    {
                        if (!connectedRoomList.Contains(i)) connectedRoomList.Add(i);
                    }
                }
            }
            return connectedRoomList;
        }
        private int GetGridIndexFromXY(float x, float y)
        {

            /*
             *  |----------------|----------------|----------------|----------------|----------------|
             *  |   0 = (-2,-2)  |   1 = (-1,-2)  |   2 = ( 0,-2)  |   3 = ( 1,-2)  |   4 = ( 2,-2)  |
             *  |----------------|----------------|----------------|----------------|----------------|
             *  |   5 = (-2,-1)  |   6 = (-1,-1)  |   7 = ( 0,-1)  |   8 = ( 1,-1)  |   9 = ( 2,-1)  |
             *  |----------------|----------------|----------------|----------------|----------------|
             *  |  10 = (-2, 0)  |  11 = (-1, 0)  |  12 = ( 0, 0)  |  13 = ( 1, 0)  |  14 = ( 2, 0)  |
             *  |----------------|----------------|----------------|----------------|----------------|
             *  |  15 = (-2, 1)  |  16 = (-1, 1)  |  17 = ( 0, 1)  |  18 = ( 1, 1)  |  19 = ( 2, 1)  |
             *  |----------------|----------------|----------------|----------------|----------------|
             *  |  20 = (-2, 2)  |  21 = (-1, 2)  |  22 = ( 0, 2)  |  23 = ( 1, 2)  |  24 = ( 2, 2)  |
             *  |----------------|----------------|----------------|----------------|----------------|
             *    
             */

            int totalRows = (GlobalMapParameters.mapSize * 2) + 1; // the map is always square so numCols is the same. Just use numRows
            // which row?
            // what if y is 25 and the map size is 50?
            // it would be 50 + 25
            // what if y is - 25?
            // it would be 50 + -25

            int rowNumber = GlobalMapParameters.mapSize + (int)y;

            // which column?
            // if y is 25 and x is 0
            // it would be (75 * (50 + 50 + 1)) + 50 + 0

            // in the graph above...
            // (2, -1)
            // row is 1
            // answer is (1 * (2 + 2 + 1)) + 2 + 2 = 9
            int index = (rowNumber * totalRows) + GlobalMapParameters.mapSize + (int)x;
            return index;
        }
        private Vector2 GetXYFromGridIndex(int i)
        {
            int totalRows = (GlobalMapParameters.mapSize * 2) + 1; // same number for columns
            int column = i % totalRows;
            float x = 0 - GlobalMapParameters.mapSize + column;
            int row = (int)Mathf.Floor((float)(i / totalRows));
            float y = 0 - GlobalMapParameters.mapSize + row;
            //if(GetGridIndexFromXY(x,y) != i)
            //{
            //    string spag = "true";
            //}
            return new Vector2(x, y);
        }
        private void InitializeGrid()
        {
            int numSquares = ((GlobalMapParameters.mapSize * 2) + 1) * ((GlobalMapParameters.mapSize * 2) + 1);
            _grid = new MapSquare[numSquares];
            
            for(int i = 0; i < numSquares; ++i)
            {
                _grid[i] = new MapSquare() { hasLand = false, isReavealed = false };
            }
        }
        private bool IsThereSpaceForTheRoom(Room room, bool padRoom = false)
        {
            /*******************************************
             * for corridors and things that connect
             * you don't want to pad. in fact, you want
             * to do the opposite so that shared walls
             * don't register as no room. However
             * for things that don't connect, like
             * regular rooms, give it a padding to keep
             * rooms from sharing walls
             * *****************************************/
            int buffer = (padRoom) ? -1 : 1;
            // adjacent rooms can share the same space, so move everything by 1 meter
            int xMin = (int)room.GetEdge(Direction.WEST) + buffer;
            int xMax = (int)room.GetEdge(Direction.EAST) - buffer;
            int yMin = (int)room.GetEdge(Direction.SOUTH) + buffer;
            int yMax = (int)room.GetEdge(Direction.NORTH) - buffer;

            //Debug.Log(string.Format("x min = {0}", xMin));
            //Debug.Log(string.Format("x max = {0}", xMax));
            //Debug.Log(string.Format("y min = {0}", yMin));
            //Debug.Log(string.Format("y max = {0}", yMax));

            if (yMin <= 0 - GlobalMapParameters.mapSize) return false;
            if (yMax >= GlobalMapParameters.mapSize) return false;
            if (xMin <= 0 - GlobalMapParameters.mapSize) return false;
            if (xMax >= GlobalMapParameters.mapSize) return false;

            for (int i = xMin; i < xMax + 1; i++)
            {
                for (int j = yMin; j < yMax + 1; j++)
                {
                    //Debug.Log(string.Format("testing space ({0}, {1})", i, j));
                    if (_grid[GetGridIndexFromXY(i, j)].hasLand) return false;
                    //Debug.Log("passed");
                }
            }
            return true;
        }
        private void PopulateGridSquares(Room room)
        {
            int xMin = (int)room.GetEdge(Direction.WEST);
            int xMax = (int)room.GetEdge(Direction.EAST);
            int yMin = (int)room.GetEdge(Direction.SOUTH);
            int yMax = (int)room.GetEdge(Direction.NORTH);
            for (int i = xMin; i < xMax + 1; i++)
            {
                for(int j = yMin; j < yMax + 1; j++)
                {
                    try
                    {
                        _grid[GetGridIndexFromXY(i, j)] = new MapSquare() { hasLand = true, isReavealed = false };
                    }
                    catch
                    {
                        Debug.LogError(string.Format("failed at space ({0}, {1})", i, j));
                    }
                }
            }
        }
        #endregion room creation methods
    }
}
