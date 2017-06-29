using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLibrary.Map
{
    public static class GlobalBuildingMaterials
    {
        public static GameObject roomEntryTrigger;

        #region tilesets
        public static GameObject tile001;
        public static float tile001Width;           // distance along the x axis
        public static float tile001Depth;           // distance along the z axis
        public static float tile001Height;          // distance along the y axis

        public static GameObject brick001;
        public static float brick001Width;           // distance along the x axis
        public static float brick001Depth;           // distance along the z axis
        public static float brick001Height;          // distance along the y axis

        public static GameObject halfBrick001;
        public static GameObject column001;
        public static GameObject torch001;

        public static GameObject door001;
        public static GameObject pentagram;

        
        #endregion tilesets

        #region characters
        public static GameObject MonsterToken;
        #endregion characters
    }
}
