using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLibrary.Map
{
    public static class MainMap
    {
        private static int _currentLevelId;
        private static Level _currentLevel;
        private static int _currentRoomId;
        private static Room _currentRoom;
        private static Transform _player;

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

    }
}
