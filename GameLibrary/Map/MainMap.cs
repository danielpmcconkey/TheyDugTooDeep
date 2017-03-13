using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLibrary.Map
{
    public class MainMap
    {
        
        public Dictionary<int, Level> levels
        {
            get { return _levels; }
        }
        Dictionary<int, Level> _levels;


        public MainMap()
        {
            _levels = new Dictionary<int, Level>();
        }
        public bool AddLevel(int levelNumber, Level newLevel)
        {
            if (_levels.ContainsKey(levelNumber))
            {
                Debug.LogError(string.Format("Level {0} already exists.", levelNumber));
                return false;
            }
            newLevel.levelNumber = levelNumber; // make sure they match
            _levels.Add(levelNumber, newLevel);
            return true;
        }
        public Level GetLevel(int levelNumber)
        {
            Level returnVal = null;
            _levels.TryGetValue(levelNumber, out returnVal);
            return returnVal;
        }

    }
}
