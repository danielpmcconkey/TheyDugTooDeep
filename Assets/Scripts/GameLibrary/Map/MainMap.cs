using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameLibrary.Helpers;

namespace GameLibrary.Map
{
    public static class MainMap
    {
        private static int _currentLevelId;
        private static Level _currentLevel;
        private static int _currentRoomId;
        private static Room _currentRoom;
        private static Transform _player;
        //private static RectTransform _minimapPanel;

        public static Dictionary<int, Level> levels
        {
            get { return _levels; }
        }
        private static Dictionary<int, Level> _levels;


        static MainMap()
        {
            _levels = new Dictionary<int, Level>();
        }
        public static void AddLevel(Level newLevel)
        {
            if (_levels.ContainsKey(newLevel.levelNumber))
            {
                Debug.LogError(string.Format("Level {0} already exists.", newLevel.levelNumber));
            }
            _levels.Add(newLevel.levelNumber, newLevel);
        }
        public static Transform GetPlayer()
        {
            return _player;
        }
        public static void SetPlayer(Transform player)
        {
            _player = player;
        }
        public static Level GetLevel(int levelNumber)
        {
            Level returnVal = null;
            _levels.TryGetValue(levelNumber, out returnVal);
            return returnVal;
        }
        public static Level GetCurrentLevel()
        {
            Level returnVal = null;
            _levels.TryGetValue(_currentLevelId, out returnVal);
            return returnVal;
        }
        public static Room GetCurrentRoom()
        {
            if(_currentLevel == null) return null;  // the portal room exists before any levels
            Room returnVal = null;
            GetCurrentLevel().rooms.TryGetValue(_currentRoomId, out returnVal);
            return returnVal;
        }
        public static void SetCurrentLevelId(int levelId)
        {
            _currentLevelId = levelId;
            _levels.TryGetValue(_currentLevelId, out _currentLevel);
        }
        public static void SetCurrentRoomId(int roomId)
        {
            if (_currentLevel == null) return;  // the portal room exists before any levels
            _currentRoomId = roomId;
            _currentLevel.rooms.TryGetValue(_currentRoomId, out _currentRoom);
        }
        //public static void SetMinimapPanel(RectTransform panel)
        //{
        //    _minimapPanel = panel;
        //}
        //public static RectTransform GetMinimapPanel() { return _minimapPanel; }
        //public static void DrawMinimap()
        //{
        //    // minimap dimensions
        //    float mm_width = _minimapPanel.rect.width;
        //    float mm_height = _minimapPanel.rect.height;
        //    //float mm_left = _minimapPanel.rect.xMin;
        //    //float mm_right = _minimapPanel.rect.xMax;
        //    //float mm_top = _minimapPanel.rect.yMax;
        //    //float mm_bottom = _minimapPanel.rect.yMin;
            
        //    // primary map dimensions
        //    float mapWidth = (GlobalMapParameters.mapSize * 2) + 1;
        //    float mapHeight = mapWidth; // map is square for now
        //    float mapBottom = 0 - GlobalMapParameters.mapSize;
        //    float mapLeft = 0 - GlobalMapParameters.mapSize;
        //    float mapTop = GlobalMapParameters.mapSize;
        //    float mapRight = GlobalMapParameters.mapSize;

        //    // converting from meters to pixels
        //    float mapUnitConversionFactorX = mapWidth / mm_width;
        //    float mapUnitConversionFactorY = mapHeight / mm_height;

        //    // set up the draw space
        //    GameObject newCanvas = new GameObject("Canvas");
        //    Canvas c = newCanvas.AddComponent<Canvas>();
        //    c.renderMode = RenderMode.ScreenSpaceOverlay;
        //    newCanvas.AddComponent<CanvasScaler>();
        //    newCanvas.AddComponent<GraphicRaycaster>();

        //    for (int i = 0; i < _currentLevel.rooms.Count; i++)
        //    {
        //        Room r = _currentLevel.rooms[i];
        //        // dimensions in main map meters
        //        float north = r.GetEdge(Direction.NORTH);
        //        float south = r.GetEdge(Direction.SOUTH);
        //        float east = r.GetEdge(Direction.EAST);
        //        float west = r.GetEdge(Direction.WEST);
        //        float roomWidth = east - west;
        //        float roomHeight = north - south;

        //        // dimensions in minimap units
        //        float offsetBottom = (mapBottom - south) * mapUnitConversionFactorY;
        //        float offsetTop = (mapTop - north) * mapUnitConversionFactorY;
        //        float offsetLeft = (mapLeft - west) * mapUnitConversionFactorX;
        //        float offsetRight = (mapRight - east) * mapUnitConversionFactorX;



        //        //float centerX = west + (roomWidth / 2);
        //        //float centerY = south + (roomHeight / 2);

        //        GameObject panel = new GameObject("Panel");
        //        panel.AddComponent<CanvasRenderer>();


        //        Image img = panel.AddComponent<Image>();
        //        RectTransform rt = img.rectTransform;
        //        //rt.anchoredPosition = new Vector2(centerX, centerY);
        //        rt.offsetMin = new Vector2(offsetLeft, offsetBottom);
        //        rt.offsetMax = new Vector2(offsetRight, offsetTop);
                
        //        img.color = Color.red;
        //        panel.transform.SetParent(newCanvas.transform, false);
        //    }
            
        //    newCanvas.transform.SetParent(_minimapPanel.transform);

        //}


    }
}
