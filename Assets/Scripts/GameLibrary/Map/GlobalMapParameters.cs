using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLibrary.Map
{
    public static class GlobalMapParameters
    {
        public static int numFloors = 6;
        public static int mapSize = 50; // the max distance from the center point in all directions
        public static int minRoomsPerFloor = 10;
        public static int maxRoomsPerFloor = 12;
        public static int minRoomSize = 8;
        public static int maxRoomSize = 25;
        public static int startingRoomRadius = 7;
        public static int roomHeight = 2;
        public static int corridorWidth = 2;
        public static int rightAngleConnectorSize = 4;
        public static int minCorridorLength = 3;
        public static int maxCorridorLength = 30;
        public static int maxRetries = 25; // used for while loops to prevent infinite looping
        public static float torchWallOffset = 0.25f;
        public static int colorVarianceMax = 10;
        public static string unwalkableLayerName = "Unwalkable";
    }
}
