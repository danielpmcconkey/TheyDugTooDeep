using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameLibrary.Helpers
{
    public enum Direction
    {
         NORTH = 0
        , EAST = 1
        , SOUTH = 2
        , WEST = 3
    }
    public struct DirectionSet
    {
        public Direction Primary;
        public Direction PrimaryOpposite;
        public Direction PerpendicularMin;
        public Direction PerpendicularMax;
    }

    public static class DirectionHelper
    {
        public static DirectionSet GetDirectionSetFromPrimary(Direction primary)
        {
            DirectionSet ds = new DirectionSet();
            ds.Primary = primary;
            ds.PrimaryOpposite = GetOppositeDirection(primary);
            int directionAdd = (primary == Direction.EAST || primary == Direction.SOUTH) ? 1 : 3; // n 3; e +1; s +1; w 3;
            ds.PerpendicularMin = (Direction)((int)(primary + directionAdd) % 4);
            ds.PerpendicularMax = GetOppositeDirection(ds.PerpendicularMin);
            return ds;
        }
        public static Direction GetOppositeDirection(Direction direction)
        {
            return (Direction)(((int)direction + 2) % 4);
        }
    }
}
