using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;

namespace GameLibrary.Map
{
    public class Room
    {
        protected int _id;
        public int id { get { return _id; } set { _id = value; } }

        protected Vector3 _northWestDown;     // the point in the room with the least x, least y, and most z
        protected Vector3 _southEastUp;       // the point in the room with the most x, most y, and least z

                                            // these values can be derived, but this is easy reference
        //private float _width;               // distance along the x axis
        //private float _depth;               // distance along the z axis
        //private float _height;              // distance along the y axis

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

        protected float _easternEdgeX;
        protected float _westernEdgeX;
        protected float _northernEdgeZ;
        protected float _southernEdgeZ;

        private List<RoomAdjacency> _adjacentRooms;
        public List<RoomAdjacency> adjacentRooms { get { return _adjacentRooms; } set { _adjacentRooms =  value; } }

        private bool _isDrawn;
        public bool isDrawn { get { return _isDrawn; } set { _isDrawn = value; } }



        public Room(int id, Vector3 northWestDown, Vector3 southEastUp, int tileSet)
        {
            _id = id;

            _adjacentRooms = new List<RoomAdjacency>();
            _isDrawn = false;

            _northWestDown = northWestDown;
            _southEastUp = southEastUp;

            _easternEdgeX = _northWestDown.x;
            _westernEdgeX = _southEastUp.x;
            _northernEdgeZ = _southEastUp.z;
            _southernEdgeZ = _northWestDown.z;



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

        public void DrawRoom()
        {
            DrawFloor();
            DrawWalls();
            DrawColumns();
            _isDrawn = true;
        }
        public float GetEdge(Direction direction)
        {
            if (direction == Direction.NORTH) return _northernEdgeZ;
            if (direction == Direction.EAST) return _easternEdgeX;
            if (direction == Direction.SOUTH) return _southernEdgeZ;
            if (direction == Direction.WEST) return _westernEdgeX;
            return 0;
        }

        private void DrawFloor()
        {
            bool offset = false;
            for (float x = _southEastUp.x; x <= _northWestDown.x; x += _tileWidth)
            {
                for (float z = _northWestDown.z; z <= _southEastUp.z; z += _tileDepth)
                {
                    float zOffset = 0;
                    if (offset) zOffset = _tileDepth / 2;
                    Transform thisTile = SetTile(new Vector3(x, _northWestDown.y, z - zOffset));
                    SetColor(thisTile);
                }
                offset = (offset) ? false : true;
            }

        }
        protected void DrawWall(Vector3 begin, Vector3 end, Direction direction)
        {
            //float xCenterOffset = _brickWidth / 2;
            float yCenterOffset = _brickHeight / 2;
            //float zCenterOffset = _brickWidth / 2;

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
            Vector3 swDown = new Vector3(_westernEdgeX, _northWestDown.y, _southernEdgeZ);
            Vector3 nwUp = new Vector3(_westernEdgeX, _southEastUp.y, _northernEdgeZ);
            Vector3 nwDown = new Vector3(_westernEdgeX, _northWestDown.y, _northernEdgeZ);
            Vector3 neUp = new Vector3(_easternEdgeX, _southEastUp.y, _northernEdgeZ);
            Vector3 seDown = new Vector3(_easternEdgeX, _northWestDown.y, _southernEdgeZ);
            Vector3 seUp = new Vector3(_easternEdgeX, _southEastUp.y, _southernEdgeZ);




            // draw west wall; SW -> NW
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(swDown, nwUp, Direction.NORTH); // just draw the wall from corner to corner
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
                    Vector3 begin = new Vector3(_westernEdgeX, swDown.y, wallPoints[j]);
                    Vector3 end = new Vector3(_westernEdgeX, nwUp.y, wallPoints[j + 1]);
                    DrawWall(begin, end, Direction.NORTH);
                }
            }


            // draw north wall; NW -> NE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(nwDown, neUp, Direction.EAST); // just draw the wall from corner to corner
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
                    Vector3 begin = new Vector3(wallPoints[j], nwDown.y, _northernEdgeZ);
                    Vector3 end = new Vector3(wallPoints[j+1], neUp.y, _northernEdgeZ);
                    DrawWall(begin, end, Direction.EAST);
                }
            }

            // draw east wall; SE -> NE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(seDown, neUp, Direction.NORTH); // just draw the wall from corner to corner
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
                    Vector3 begin = new Vector3(_easternEdgeX, seDown.y, wallPoints[j]);
                    Vector3 end = new Vector3(_easternEdgeX, neUp.y, wallPoints[j + 1]);
                    DrawWall(begin, end, Direction.NORTH);
                }
            }

            // draw south wall; SW -> SE
            if (_adjacentRooms == null || _adjacentRooms.Count == 0)
                DrawWall(swDown, seUp, Direction.EAST); // just draw the wall from corner to corner
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
                    Vector3 begin = new Vector3(wallPoints[j], swDown.y, _southernEdgeZ);
                    Vector3 end = new Vector3(wallPoints[j + 1], seUp.y, _southernEdgeZ);
                    DrawWall(begin, end, Direction.EAST);
                }
            }
        }


        //private void DrawWalls()
        //{
        //    bool offset = false;
        //    for (float y = _northWestDown.y + _tileHeight + (_brickHeight / 2); y <= _southEastUp.y; y += _brickHeight)
        //    {
        //        // go around the perimeter

        //        // build the north wall at this level, going west to east
        //        for (float x = _southEastUp.x + _brickWidth; x <= _northWestDown.x + _brickWidth; x += _brickDepth)
        //        {
        //            float xOffset = 0;
        //            if (offset) xOffset = _brickDepth / 2;
        //            Transform thisBrick = SetBrick(new Vector3(x + xOffset, y, _southEastUp.z - (_brickWidth / 2)), Quaternion.Euler(0, 0, -90));
        //        }
        //        // build the east wall at this level, going north to south
        //        for (float z = _southEastUp.z - (_brickDepth / 2); z >= _northWestDown.z + (_brickDepth / 2); z -= _brickDepth)
        //        {
        //            float zOffset = 0;
        //            if (offset) zOffset = _brickDepth / 2;
        //            Transform thisBrick = SetBrick(new Vector3(_northWestDown.x + (_brickWidth / 2), y, z - zOffset), Quaternion.Euler(-90, 0, 0));
        //        }
        //        // build the south wall at this level, going east to west
        //        for (float x = _northWestDown.x; x >= _southEastUp.x; x -= _brickDepth)
        //        {
        //            float xOffset = 0;
        //            if (offset) xOffset = _brickDepth / 2;
        //            Transform thisBrick = SetBrick(new Vector3(x - xOffset, y, _northWestDown.z - (_brickWidth / 2)), Quaternion.Euler(0, 0, -90));
        //        }
        //        // build the west wall at this level, going south to north
        //        for (float z = _northWestDown.z; z <= _southEastUp.z - _brickWidth; z += _brickDepth)
        //        {
        //            float zOffset = 0;
        //            if (offset) zOffset = _brickDepth / 2;
        //            Transform thisBrick = SetBrick(new Vector3(_southEastUp.x + (_brickWidth / 2), y, z + zOffset), Quaternion.Euler(90, 0, 0));

        //        }

        //        offset = (offset) ? false : true;
        //    }
        //}

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