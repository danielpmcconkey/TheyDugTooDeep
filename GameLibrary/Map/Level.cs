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


        public Level(Vector3 startingPoint)
        {
            InitializeGrid();
            _rooms = new Dictionary<int, Room>();
            _startingPoint = startingPoint;
        }

        public bool CreateRooms()
        {
            int numRooms = RNG.getRandomInt(GlobalMapParameters.minRoomsPerFloor, GlobalMapParameters.maxRoomsPerFloor);
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
             * *******************************/
            Room starterRoom = new Room(1, new Vector3(eastEdge, 0, southEdge), new Vector3(westEdge, GlobalMapParameters.roomHeight, northEdge), tileSet);
            PopulateGridSquares(starterRoom);
            _rooms.Add(1, starterRoom);
            starterRoom.DrawRoom();

            // starter corridor
            Corridor starterCorridor = CreateCorridorFromRoom(2, starterRoom, tileSet);
            _rooms.Add(2, starterCorridor);
            starterCorridor.DrawRoom();

            _rooms.Add(3, CreateRoomFromCorridor(3, starterCorridor, tileSet));
            _rooms[3].DrawRoom();

            if (_rooms[3] == null) Debug.LogError("room 3 is null");

            int foreverLoop = 0;

            for (int i = 2; i < numRooms; i++) // added 2 rooms already
            {
                foreverLoop++;
                if (foreverLoop > 100) i = numRooms + 100; // todo: remove forever loop
                Debug.Log(string.Format("i = {0}", i));
                Corridor corridor = CreateCorridorFromRoom(i * 2, _rooms[(i * 2) - 1], tileSet);
                if (corridor != null)
                {
                    //Debug.Log("coordior created");
                    _rooms.Add(i * 2, corridor);
                    corridor.DrawRoom(); // todo: remove corridor.draw when creating

                    Room room = CreateRoomFromCorridor((i * 2) + 1, corridor, tileSet);
                    if (room != null)
                    {
                        _rooms.Add((i * 2) + 1, room);
                        room.DrawRoom(); // todo: remove room.draw when creating
                    }
                    else
                    {
                        _rooms.Remove(i * 2); // remove the corridor that lead to this
                        i -= 1; // roll back the clock
                        break;
                    }
                }
                else
                {
                    i -= 1; // roll back the clock
                    break;
                }
            }



            //_rooms.Add(4, CreateCorridorFromRoom(4, _rooms[3], tileSet));
            //_rooms.Add(5, CreateRoomFromCorridor(5, (Corridor)_rooms[4], tileSet));
            //_rooms.Add(6, CreateCorridorFromRoom(6, _rooms[5], tileSet));
            //_rooms.Add(7, CreateRoomFromCorridor(7, (Corridor)_rooms[6], tileSet));
            //_rooms.Add(8, CreateCorridorFromRoom(8, _rooms[7], tileSet));
            //_rooms.Add(9, CreateRoomFromCorridor(9, (Corridor)_rooms[8], tileSet));


            //starterRoom.DrawRoom();
            //foreach (RoomAdjacency roomAdjacency in starterRoom.adjacentRooms)
            //{
            //    roomAdjacency.room.DrawRoom();
            //}

            //for (int i = 1; i < _rooms.Count; i++) _rooms[i].DrawRoom();
            //_rooms[3].DrawRoom();
            //_rooms[4].DrawRoom();
            //_rooms[5].DrawRoom();
            //_rooms[6].DrawRoom();
            //_rooms[7].DrawRoom();
            //_rooms[8].DrawRoom();
            //_rooms[9].DrawRoom();



            return true;
            

        }

        private Room CreateRoomFromCorridor(int id, Corridor corridor, int tileSet)
        {
            int roomWidth = RNG.getRandomInt(GlobalMapParameters.minRoomSize, GlobalMapParameters.maxRoomSize); // distance along x axis
            int roomDepth = RNG.getRandomInt(GlobalMapParameters.minRoomSize, GlobalMapParameters.maxRoomSize); // distance along z axis
            //Debug.LogWarning(string.Format("room width = {0}", roomWidth));
            //Debug.LogWarning(string.Format("room depth = {0}", roomDepth));

            Direction directionAwayFromPreviousRoom = corridor.GetDirection();
            Direction connectingWall = GetOppositeDirection(directionAwayFromPreviousRoom);

            // todo: clean up connectingPointRelative to make the min and max values variables
            float connectingPointRelative = (connectingWall == Direction.NORTH || connectingWall == Direction.SOUTH) ?
                (float)RNG.getRandomInt((1 + GlobalMapParameters.corridorWidth / 2), roomWidth - 2*(1 + GlobalMapParameters.corridorWidth / 2)) :
                (float)RNG.getRandomInt((1 + GlobalMapParameters.corridorWidth / 2), roomDepth - 2*(1 + GlobalMapParameters.corridorWidth / 2)); // returns a point relative to that wall's starting corner

            //Debug.LogWarning(string.Format("Connecting wall = {0}", connectingWall.ToString()));
            //Debug.LogWarning(string.Format("Connecting point relative = {0}", connectingPointRelative));

            Room room;
            float northEdge = 0;
            float eastEdge = 0;
            float southEdge = 0;
            float westEdge = 0;

            switch (connectingWall)
            {
                case Direction.NORTH:
                    northEdge = corridor.GetEdge(Direction.SOUTH);
                    southEdge = northEdge - roomDepth;
                    eastEdge = corridor.GetEdge(Direction.WEST) + (GlobalMapParameters.corridorWidth / 2) + (roomWidth - connectingPointRelative);
                    westEdge = corridor.GetEdge(Direction.WEST) + (GlobalMapParameters.corridorWidth / 2) - (roomWidth - connectingPointRelative);
                    break;
                case Direction.SOUTH:
                    southEdge = corridor.GetEdge(Direction.NORTH);
                    northEdge = southEdge + roomDepth;
                    eastEdge = corridor.GetEdge(Direction.WEST) + (GlobalMapParameters.corridorWidth / 2) + (roomWidth - connectingPointRelative);
                    westEdge = corridor.GetEdge(Direction.WEST) + (GlobalMapParameters.corridorWidth / 2) - (roomWidth - connectingPointRelative);
                    break;
                case Direction.WEST:
                    westEdge = corridor.GetEdge(Direction.EAST);
                    eastEdge = westEdge + roomWidth;
                    northEdge = corridor.GetEdge(Direction.SOUTH) + (GlobalMapParameters.corridorWidth / 2) + (roomDepth - connectingPointRelative);
                    southEdge = corridor.GetEdge(Direction.SOUTH) + (GlobalMapParameters.corridorWidth / 2) - (roomDepth - connectingPointRelative);
                    break;
                case Direction.EAST:
                    eastEdge = corridor.GetEdge(Direction.WEST);
                    westEdge = eastEdge - roomWidth;
                    northEdge = corridor.GetEdge(Direction.SOUTH) + (GlobalMapParameters.corridorWidth / 2) + (roomDepth - connectingPointRelative);
                    southEdge = northEdge - roomDepth;
                    break;
            }
            //Debug.LogWarning(string.Format("north edge = {0}", northEdge));
            //Debug.LogWarning(string.Format("south edge = {0}", southEdge));
            //Debug.LogWarning(string.Format("east edge = {0}", eastEdge));
            //Debug.LogWarning(string.Format("west edge = {0}", westEdge));

            room = new Room(
                id,
                new Vector3(eastEdge, 0, southEdge),
                new Vector3(westEdge, GlobalMapParameters.roomHeight, northEdge),
                tileSet
                );
            room.adjacentRooms.Add(new RoomAdjacency() { room = corridor, adjacentWall = connectingWall });
            corridor.adjacentRooms.Add(new RoomAdjacency() { room = room, adjacentWall = GetOppositeDirection(directionAwayFromPreviousRoom) });

            if (!IsThereSpaceForTheRoom(room)) return null;
            PopulateGridSquares(room);
            return room;
            
        }
        private Corridor CreateCorridorFromRoom(int id, Room room, int tileSet)
        {
            int corridorLength = RNG.getRandomInt(GlobalMapParameters.minCorridorLength, GlobalMapParameters.maxCorridorLength);
            Direction corridorDirectionAwayFromRoom = (Direction)RNG.getRandomInt(0, 3);
            //if (id == 8) Debug.LogWarning(string.Format("direction away from wall: {0}", corridorDirectionAwayFromRoom));

            // wrongWay stops it from going back the way it came
            bool wrongWay = false;
            foreach (RoomAdjacency adjacency in room.adjacentRooms)
            {
                if (corridorDirectionAwayFromRoom == (adjacency).adjacentWall) wrongWay = true;
            }
            if (wrongWay) corridorDirectionAwayFromRoom = (Direction)(((int)corridorDirectionAwayFromRoom + 1) % 4);


            Corridor corridor;
            float corridorSouthernEdge = 0;
            float corridorNorthernEdge = 0;
            float corridorCenter;
            float corridorEasternEdge = 0;
            float corridorWesternEdge = 0;
            float northEdge = room.GetEdge(Direction.NORTH);
            float eastEdge = room.GetEdge(Direction.EAST);
            float southEdge = room.GetEdge(Direction.SOUTH);
            float westEdge = room.GetEdge(Direction.WEST);


            //(float)RNG.getRandomInt((1 + GlobalMapParameters.corridorWidth / 2), roomWidth - 2 * (1 + GlobalMapParameters.corridorWidth / 2)) :

            switch (corridorDirectionAwayFromRoom)
            {
                case Direction.NORTH:
                    corridorSouthernEdge = northEdge;
                    corridorNorthernEdge = northEdge + corridorLength;
                    corridorCenter = (float)RNG.getRandomInt((int)westEdge + 1 + (GlobalMapParameters.corridorWidth / 2), (int)eastEdge - 1 - (GlobalMapParameters.corridorWidth / 2));
                    corridorEasternEdge = corridorCenter + (GlobalMapParameters.corridorWidth / 2);
                    corridorWesternEdge = corridorCenter - (GlobalMapParameters.corridorWidth / 2);
                    break;
                case Direction.SOUTH:
                    corridorSouthernEdge = southEdge - corridorLength;
                    corridorNorthernEdge = southEdge;
                    corridorCenter = (float)RNG.getRandomInt((int)westEdge + 1 + (GlobalMapParameters.corridorWidth / 2), (int)eastEdge - 1 - (GlobalMapParameters.corridorWidth / 2));
                    corridorEasternEdge = corridorCenter + (GlobalMapParameters.corridorWidth / 2);
                    corridorWesternEdge = corridorCenter - (GlobalMapParameters.corridorWidth / 2);
                    break;
                case Direction.WEST:
                    corridorEasternEdge = westEdge;
                    corridorWesternEdge = westEdge - corridorLength;
                    corridorCenter = (float)RNG.getRandomInt((int)southEdge + 1 + (GlobalMapParameters.corridorWidth / 2), (int)northEdge - 1 - (GlobalMapParameters.corridorWidth / 2));
                    corridorNorthernEdge = corridorCenter + (GlobalMapParameters.corridorWidth / 2);
                    corridorSouthernEdge = corridorCenter - (GlobalMapParameters.corridorWidth / 2);
                    break;
                case Direction.EAST:
                    corridorWesternEdge = eastEdge;
                    corridorEasternEdge = eastEdge + corridorLength;
                    corridorCenter = (float)RNG.getRandomInt((int)southEdge + 1 + (GlobalMapParameters.corridorWidth / 2), (int)northEdge - 1 - (GlobalMapParameters.corridorWidth / 2));
                    corridorNorthernEdge = corridorCenter + (GlobalMapParameters.corridorWidth / 2);
                    corridorSouthernEdge = corridorCenter - (GlobalMapParameters.corridorWidth / 2);
                    break;
            }
            //if (id == 8)
            //{
            //    Debug.LogWarning(string.Format("direction away from wall: {0}", corridorDirectionAwayFromRoom));
            //    Debug.LogWarning(string.Format("north edge = {0}", corridorNorthernEdge));
            //    Debug.LogWarning(string.Format("south edge = {0}", corridorSouthernEdge));
            //    Debug.LogWarning(string.Format("east edge = {0}", corridorEasternEdge));
            //    Debug.LogWarning(string.Format("west edge = {0}", corridorWesternEdge));
            //}

            corridor = new Corridor(
                id,
                new Vector3(corridorEasternEdge, 0, corridorSouthernEdge),
                new Vector3(corridorWesternEdge, GlobalMapParameters.roomHeight, corridorNorthernEdge),
                tileSet,
                corridorDirectionAwayFromRoom
                );

            if (!IsThereSpaceForTheRoom(corridor))
            {
                corridor.DrawRoom();
                return null; // check if there's space before adding the adjacencies
            }
            room.adjacentRooms.Add(new RoomAdjacency() { room = corridor, adjacentWall = corridorDirectionAwayFromRoom });
            corridor.adjacentRooms.Add(new RoomAdjacency() { room = room, adjacentWall = GetOppositeDirection(corridorDirectionAwayFromRoom) });

            PopulateGridSquares(corridor);
            return corridor;

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
        private bool IsThereSpaceForTheRoom(Room room)
        {
            // adjacent rooms can share the same space, so move everything by 1 meter
            int xMin = (int)room.GetEdge(Direction.WEST) + 1;
            int xMax = (int)room.GetEdge(Direction.EAST) - 1;
            int yMin = (int)room.GetEdge(Direction.SOUTH) + 1;
            int yMax = (int)room.GetEdge(Direction.NORTH) - 1;

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
