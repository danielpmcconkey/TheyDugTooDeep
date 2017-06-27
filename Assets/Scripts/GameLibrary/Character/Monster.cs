using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Map;

namespace GameLibrary.Character
{

    public class Monster
    {
        protected Transform _transform;
        protected bool _isAlive;
        protected int _roomId;
        protected int _idInRoom;

        public Monster(Transform transform, int roomId, int idInRoom)
        {
            _transform = transform;
            _isAlive = true;
            _roomId = roomId;
            _idInRoom = idInRoom;
        }
        public Vector3 GetPosition() { return _transform.position; }
        public void Kill()
        {
            _isAlive = false;
            UnityEngine.Object.Destroy(_transform.gameObject);
            MainMap.GetCurrentRoom().UpdateKillSheet();
            if (MainMap.GetCurrentRoom().IsCleared()) MainMap.GetCurrentRoom().UnlockDoors();
        }
        public bool IsAlive() { return _isAlive; }
    }
}