using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;

namespace GameLibrary.Map
{
    public class Corridor : Room
    {
        private Direction _direction;

        public Corridor(int id, Vector3 northWestDown, Vector3 southEastUp, int tileSet, Direction direction) : base (id, northWestDown, southEastUp, tileSet)
        {
            _direction = direction;
        }
        public Direction GetDirection()
        {
            return _direction;
        }
        protected override void DrawWalls()
        {
            Vector3 swDown = new Vector3(_westernEdgeX, _northWestDown.y, _southernEdgeZ);
            Vector3 nwUp = new Vector3(_westernEdgeX, _southEastUp.y, _northernEdgeZ);
            Vector3 nwDown = new Vector3(_westernEdgeX, _northWestDown.y, _northernEdgeZ);
            Vector3 neUp = new Vector3(_easternEdgeX, _southEastUp.y, _northernEdgeZ);
            Vector3 seDown = new Vector3(_easternEdgeX, _northWestDown.y, _southernEdgeZ);
            Vector3 seUp = new Vector3(_easternEdgeX, _southEastUp.y, _southernEdgeZ);

            if(_direction == Direction.NORTH || _direction == Direction.SOUTH)
            {
                // Debug.LogWarning("north -> south");
                // draw west wall; SW -> NW
                DrawWall(swDown, nwUp, Direction.NORTH);
                // draw east wall; SE -> NE
                DrawWall(seDown, neUp, Direction.NORTH);
            }
            if (_direction == Direction.EAST || _direction == Direction.WEST)
            {
                // Debug.LogWarning("east -> west");
                // draw north wall; NW -> NE
                DrawWall(nwDown, neUp, Direction.EAST);
                // draw south wall; SW -> SE
                DrawWall(swDown, seUp, Direction.EAST);
            }
        }



    }
}