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


        public Level(Vector3 startingPoint)
        {
            InitializeGrid();
            _rooms = new Dictionary<int, Room>();
            _startingPoint = startingPoint;
            _numMainRooms = RNG.getRandomInt(GlobalMapParameters.minRoomsPerFloor, GlobalMapParameters.maxRoomsPerFloor);
        }

        public bool CreateRooms()
        {
            int tileSet = 1; // todo: do something about tilesets

            // starter room
            // starter room is always startingRoomRadius meters in each direction around the starting point
            float northEdge = _startingPoint.z + GlobalMapParameters.startingRoomRadius;
            float southEdge = _startingPoint.z - GlobalMapParameters.startingRoomRadius;
            float eastEdge = _startingPoint.x + GlobalMapParameters.startingRoomRadius;
            float westEdge = _startingPoint.x - GlobalMapParameters.startingRoomRadius;

            /*********************************
             * note to self, I think the 
             * northwestdown and southeastup
             * variables are misnamed.
             * todo: fix nwdown and swup
             * *******************************/
            Room starterRoom = new Room(0, new Vector3(eastEdge, 0, southEdge), new Vector3(westEdge, GlobalMapParameters.roomHeight, northEdge), tileSet);
            PopulateGridSquares(starterRoom);
            _rooms.Add(0, starterRoom);

            for (int i = 1; i < _numMainRooms; i++) // added 1 room already
            {
                Room room = CreateRoom(i, tileSet);
                _rooms.Add(i, room);

            }
            CreateCorridors(tileSet);
            CreateRightAngleConnections(tileSet);

            for (int i = 0; i < _rooms.Count; i++)
            {
                _rooms[i].DrawRoom();
            }

            return true;
            

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
                                                new Vector3(cpEastEdge, 0, cpSouthEdge),
                                                new Vector3(cpWestEdge, GlobalMapParameters.roomHeight, cpNorthEdge),
                                                tileSet
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
                                                new Vector3(zLegEastEdge, 0, zLegSouthEdge),
                                                new Vector3(zLegWestEdge, GlobalMapParameters.roomHeight, zLegNorthEdge),
                                                tileSet,
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
                                                    new Vector3(xLegEastEdge, 0, xLegSouthEdge),
                                                    new Vector3(xLegWestEdge, GlobalMapParameters.roomHeight, xLegNorthEdge),
                                                    tileSet,
                                                    Direction.EAST
                                                    );
                                    if (IsThereSpaceForTheRoom(xLeg))
                                    {
                                        // we have a match
                                        PopulateGridSquares(centerPoint);
                                        PopulateGridSquares(zLeg);
                                        PopulateGridSquares(xLeg);

                                        // room adjacencies
                                        centerPoint.adjacentRooms.Add(
                                            new RoomAdjacency() { room = zLeg, adjacentWall = (sourceZ > targetZ) ? Direction.NORTH : Direction.SOUTH }
                                            );
                                        centerPoint.adjacentRooms.Add(
                                            new RoomAdjacency() { room = xLeg, adjacentWall = (sourceX > targetX) ? Direction.WEST : Direction.EAST }
                                            );

                                        source.adjacentRooms.Add(
                                            new RoomAdjacency() { room = zLeg, adjacentWall = (sourceZ > targetZ) ? Direction.SOUTH : Direction.NORTH }
                                            );
                                        target.adjacentRooms.Add(
                                            new RoomAdjacency() { room = xLeg, adjacentWall = (sourceX > targetX) ? Direction.EAST : Direction.WEST }
                                            );
                                        // add target adjacency for zleg
                                        // add the xleg
                                        centerPoint.id = _rooms.Count;
                                        _rooms.Add(_rooms.Count, centerPoint);
                                        zLeg.id = _rooms.Count;
                                        _rooms.Add(_rooms.Count, zLeg);
                                        xLeg.id = _rooms.Count;
                                        _rooms.Add(_rooms.Count, xLeg);
                                        // add the room connections
                                        source.connectedRooms.Add(target);
                                        target.connectedRooms.Add(source);

                                    } // end IsThereSpaceForTheRoom(xLeg)
                                } // end IsThereSpaceForTheRoom(zLeg)
                            } // end IsThereSpaceForTheRoom(centerPoint)
                        } // end rooms might work

                    } // end if j != i
                } // end for j loop
                
                    
            } // end for i loop
        }
        private void CreateCorridors(int tileSet)
        {
            for (int i = 0; i < _numMainRooms; i++) // can't foreach the list because you add to it while you're doing it
            {
                if (_rooms[i].adjacentRooms.Count == 0)
                {
                    RoomConnection connection = FindRoomToConnect(_rooms[i], tileSet);
                    if (connection != null)
                    {
                        int id = _rooms.Count;
                        connection.corridor.id = id;
                        _rooms.Add(id, connection.corridor);
                        connection.starterRoom.adjacentRooms.Add(new RoomAdjacency() {
                            room = connection.corridor, adjacentWall = connection.corridor.direction });
                        connection.starterRoom.connectedRooms.Add(connection.connectedRoom);
                        connection.connectedRoom.adjacentRooms.Add(new RoomAdjacency()
                        {
                            room = connection.corridor,
                            adjacentWall = GetOppositeDirection(connection.corridor.direction)
                        });
                        connection.connectedRoom.connectedRooms.Add(connection.starterRoom);
                    }
                }
            }
            // try for a second connection where possible todo: DRY this code
            for (int i = 0; i < _numMainRooms; i++) // can't foreach the list because you add to it while you're doing it
            {
                RoomConnection connection = FindRoomToConnect(_rooms[i], tileSet);
                if (connection != null)
                {
                    int id = _rooms.Count;
                    connection.corridor.id = id;
                    _rooms.Add(id, connection.corridor);
                    connection.starterRoom.adjacentRooms.Add(new RoomAdjacency() { room = connection.corridor, adjacentWall = connection.corridor.direction });
                    connection.connectedRoom.adjacentRooms.Add(new RoomAdjacency()
                    {
                        room = connection.corridor,
                        adjacentWall = GetOppositeDirection(connection.corridor.direction)
                    });
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
                Direction primaryDirection = direction;
                Direction primaryDirectionOpposite = GetOppositeDirection(primaryDirection);
                int directionAdd = (primaryDirection == Direction.EAST || primaryDirection == Direction.SOUTH) ? 1 : 3; // n 3; e +1; s +1; w 3;
                Direction perpendicularDirectionMin = (Direction)((int)(primaryDirection + directionAdd) % 4);
                Direction perpendicularDirectionMax = GetOppositeDirection(perpendicularDirectionMin);

                // now try each room to see if it can fit.
                foreach (KeyValuePair<int, Room> roomEntry in _rooms)
                {
                    Room candidate = roomEntry.Value;
                    bool alreadyConnected = false;
                    if (room.connectedRooms != null)
                    {
                        foreach (Room alreadyConnectedRoom in room.connectedRooms)
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

                            if (candidate.GetEdge(perpendicularDirectionMin) < room.GetEdge(perpendicularDirectionMax) - 1)
                                if (candidate.GetEdge(perpendicularDirectionMax) > room.GetEdge(perpendicularDirectionMin) + 1)
                                    if (candidate.GetEdge(primaryDirectionOpposite) >= room.GetEdge(primaryDirection) + GlobalMapParameters.minCorridorLength)
                                        if (candidate.GetEdge(primaryDirectionOpposite) <= room.GetEdge(primaryDirection) + GlobalMapParameters.maxCorridorLength)
                                        {
                                            /*
                                             * we have a match, as long as nothing else is in the way
                                             * find the east/west bounds of the corridor
                                             * how much overlap is there?
                                             * */

                                            float corridorMinX;
                                            float corridorMaxX;

                                            corridorMinX = (candidate.GetEdge(perpendicularDirectionMin) <= room.GetEdge(perpendicularDirectionMin)) ?
                                                room.GetEdge(perpendicularDirectionMin) :
                                                candidate.GetEdge(perpendicularDirectionMin);
                                            corridorMaxX = (candidate.GetEdge(perpendicularDirectionMax) >= room.GetEdge(perpendicularDirectionMax)) ?
                                                room.GetEdge(perpendicularDirectionMax) :
                                                candidate.GetEdge(perpendicularDirectionMax);

                                            float corridorLesserEdge = RNG.getRandomInt((int)corridorMinX, (int)corridorMaxX - GlobalMapParameters.corridorWidth);
                                            float corridorGreaterEdge = corridorLesserEdge + GlobalMapParameters.corridorWidth;
                                            float corridorTargetEdge = candidate.GetEdge(primaryDirectionOpposite);
                                            float corridorSourceEdge = room.GetEdge(primaryDirection);

                                            float northEdge = 0;
                                            float southEdge = 0;
                                            float eastEdge = 0;
                                            float westEdge = 0;

                                            if (primaryDirection == Direction.NORTH || primaryDirection == Direction.SOUTH)
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
                                                new Vector3(eastEdge, 0, southEdge),
                                                new Vector3(westEdge, GlobalMapParameters.roomHeight, northEdge),
                                                tileSet,
                                                primaryDirection
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
                    new Vector3(eastEdge, 0, southEdge),
                    new Vector3(westEdge, GlobalMapParameters.roomHeight, northEdge),
                    tileSet
                    );

                if (IsThereSpaceForTheRoom(room, true)) roomPlaced = true;
            }


            PopulateGridSquares(room);
            return room;

        }


        private Direction GetOppositeDirection(Direction direction)
        {
            return (Direction)(((int)direction + 2) % 4);
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
    }
}
