using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;

namespace GameLibrary.Map
{
    public class Corridor : Room
    {
        private Direction _direction;
        public Direction direction { get { return _direction; } set { _direction = value;  } }

        public Corridor(int id, Vector3 southWestDown, Vector3 northEastUp, int tileSet, Direction direction) : base (id, southWestDown, northEastUp, tileSet)
        {
            _direction = direction;
        }
        public Direction GetDirection()
        {
            return _direction;
        }
        protected override void DrawWalls()
        {
            Vector3 nwUp = new Vector3(_westernEdgeX, _ceilingY, _northernEdgeZ);
            Vector3 nwDown = new Vector3(_westernEdgeX, _floorY, _northernEdgeZ);
            Vector3 seDown = new Vector3(_easternEdgeX, _floorY, _southernEdgeZ);
            Vector3 seUp = new Vector3(_easternEdgeX, _ceilingY, _southernEdgeZ);

            if(_direction == Direction.NORTH || _direction == Direction.SOUTH)
            {
                // draw west wall; SW -> NW
                DrawWall(_southWestDown, nwUp, Direction.NORTH);
                // draw east wall; SE -> NE
                DrawWall(seDown, _northEastUp, Direction.NORTH);
            }
            if (_direction == Direction.EAST || _direction == Direction.WEST)
            {
                // draw north wall; NW -> NE
                DrawWall(nwDown, _northEastUp, Direction.EAST);
                // draw south wall; SW -> SE
                DrawWall(_southWestDown, seUp, Direction.EAST);
            }
        }



    }
}