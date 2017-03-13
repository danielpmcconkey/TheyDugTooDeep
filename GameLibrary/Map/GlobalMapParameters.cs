﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLibrary.Map
{
    public static class GlobalMapParameters
    {
        // todo supply global map parameters from Unity
        public static int mapSize = 50; // the max distance from the center point in all directions
        public static int minRoomsPerFloor = 12;
        public static int maxRoomsPerFloor = 16;
        public static int minRoomSize = 5;
        public static int maxRoomSize = 20;
        public static int startingRoomRadius = 5;
        public static int roomHeight = 2;
        public static int corridorWidth = 2;
        public static int minCorridorLength = 3;
        public static int maxCorridorLength = 12;
    }
}
