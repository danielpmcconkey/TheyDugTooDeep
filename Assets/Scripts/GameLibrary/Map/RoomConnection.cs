using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GameLibrary.Map
{
    public class RoomConnection : IEquatable<RoomConnection>
    {
        public Room starterRoom;
        public Corridor corridor;
        public Room connectedRoom;

        #region comparison methods
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RoomConnection objAsRoomConnection = obj as RoomConnection;
            if (objAsRoomConnection == null) return false;
            else return Equals(objAsRoomConnection);
        }
        public bool Equals(RoomConnection r)
        {
            if (starterRoom != null && r.starterRoom == null) return false;
            if (starterRoom != null && r.starterRoom != starterRoom) return false;
            if (corridor != null && r.corridor == null) return false;
            if (corridor != null && r.corridor != corridor) return false;
            if (connectedRoom != null && r.connectedRoom == null) return false;
            if (connectedRoom != null && r.connectedRoom != connectedRoom) return false;

            return true;
        }
        //public override int GetHashCode()
        //{
        //    return id;
        //}
        #endregion comparison methods
    }
}
