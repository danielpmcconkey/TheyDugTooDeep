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

        public Corridor(int id, Vector3 southWestDown, Vector3 northEastUp, int tileSet, Color baseColor, Direction direction) : base (id, southWestDown, northEastUp, tileSet, baseColor)
        {
            _direction = direction;
        }
        public Direction GetDirection()
        {
            return _direction;
        }
        //public override void DrawRoom()
        //{
        //    base.DrawRoom();
            

        //}


        protected override Transform DrawWalls()
        {
            Transform container = new GameObject().transform;
            container.name = string.Format("Room {0} walls container", id);

            Vector3 nwUp = new Vector3(_westernEdgeX, _ceilingY, _northernEdgeZ);
            Vector3 nwDown = new Vector3(_westernEdgeX, _floorY, _northernEdgeZ);
            Vector3 seDown = new Vector3(_easternEdgeX, _floorY, _southernEdgeZ);
            Vector3 seUp = new Vector3(_easternEdgeX, _ceilingY, _southernEdgeZ);

            if(_direction == Direction.NORTH || _direction == Direction.SOUTH)
            {
                // draw west wall; SW -> NW
                Transform wallContainer = DrawWall(_southWestDown, nwUp, Direction.NORTH);
                wallContainer.parent = container;
                // draw east wall; SE -> NE
                wallContainer = DrawWall(seDown, _northEastUp, Direction.NORTH);
                wallContainer.parent = container;
            }
            if (_direction == Direction.EAST || _direction == Direction.WEST)
            {
                // draw north wall; NW -> NE
                Transform wallContainer = DrawWall(nwDown, _northEastUp, Direction.EAST);
                wallContainer.parent = container;
                // draw south wall; SW -> SE
                wallContainer = DrawWall(_southWestDown, seUp, Direction.EAST);
                wallContainer.parent = container;
            }

            return container;
        }

        protected override Transform DrawColumns()
        {
            // do nothing, because a corridor doesn't need columns, thanks to door frames having them
            return null;
        }

    }
}