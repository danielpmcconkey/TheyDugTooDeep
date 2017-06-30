using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Map;
using GameLibrary.Helpers;

public class CharacterCollisions : MonoBehaviour {

    private int _lastTriggeredRoom = -1;
    private float _lastTeleport;
    public float teleportCooldownSeconds = 5f;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(string.Format("Collided with object. Name: {0}. Tag: {1}.", other.gameObject.name, other.gameObject.tag));
        switch(other.gameObject.tag)
        {
            case "RoomEntryTrigger":
                CollideWithRoom(other);
                break;
            case "Teleporter":
                if (Time.time >= _lastTeleport + teleportCooldownSeconds)
                {
                    Teleport(other.transform.parent.name);
                    _lastTeleport = Time.time;
                }
                break;
            case "LevelAdvanceTeleporter":
                if (Time.time >= _lastTeleport + teleportCooldownSeconds)
                {
                    LevelAdvanceTeleport(other.transform.parent.name);
                    _lastTeleport = Time.time;
                }
                break;
        }



    }
    private void OnTriggerExit(Collider other)
    {
        //Debug.Log(string.Format("Exited from object. Name: {0}. Tag: {1}.", other.gameObject.name, other.gameObject.tag));
    }

    private void Teleport(string tName)
    {
        if (tName == string.Empty || tName == "") return;   // no destination. one way teleporter
        string[] nameSplit = tName.Split('|');
        Vector3 to = new Vector3(int.Parse(nameSplit[1]), int.Parse(nameSplit[2]), int.Parse(nameSplit[3]));
        //StartCoroutine("MoveToPosition", to);

        if (MainMap.GetCurrentLevel() != null)
        {
            foreach (Teleporter t in MainMap.GetCurrentRoom().GetTeleporters())
            {
                if (t.destinationRoom != null && !t.destinationRoom.IsRendered()) t.destinationRoom.RenderRoomObjects(true);
            }
        }
        transform.position = to;
    }
    private void LevelAdvanceTeleport(string tName)
    {
        string[] nameSplit = tName.Split('|');
        Vector3 to = new Vector3(int.Parse(nameSplit[1]), int.Parse(nameSplit[2]), int.Parse(nameSplit[3]));
        int newLevelId = int.Parse(nameSplit[4]);

        Level oldLevel = MainMap.GetCurrentLevel();
        oldLevel.RenderEntireLevel(false);
        oldLevel.RenderMinimap(false);
        MainMap.SetCurrentLevelId(newLevelId);
        MainMap.SetCurrentRoomId(0);
        Level newLevel = MainMap.GetCurrentLevel();
        newLevel.RenderMinimap(true);
        MainMap.GetCurrentRoom().RenderRoomObjects(true);
        newLevel.RenderDoors(true);

        transform.position = to;
    }

    //private IEnumerator MoveToPosition(Vector3 target)
    //{
    //    Vector3 startPos = transform.position;
    //    while (Vector3.Distance(startPos, target) > 0.005f)
    //    {
    //        transform.position = Vector3.MoveTowards(startPos, target, 7.0f * Time.deltaTime);
    //        yield return null;
    //    }
    //}
    private void CollideWithRoom(Collider other)
    {
        if (MainMap.GetCurrentLevel() == null) return; // the portal room exists before anything else does, so nope the fuck outta here


        string[] nameSplit = other.name.Split(':');
        int thisRoomId = -1;
        int.TryParse(nameSplit[1], out thisRoomId);
        if (thisRoomId >= 0 && thisRoomId != _lastTriggeredRoom)
        {
            //Debug.Log("entered room ID:" + thisRoomId);
            MainMap.SetCurrentRoomId(thisRoomId);
            _lastTriggeredRoom = thisRoomId;
            if (MainMap.GetCurrentRoom().IsCleared()) MainMap.GetCurrentRoom().UnlockDoors();




            List<Room> roomsThatShouldBeDrawn = new List<Room>();

            Room thisRoom = MainMap.GetCurrentRoom();
            roomsThatShouldBeDrawn.Add(thisRoom);


            List<RoomAdjacency> adjacencies = thisRoom.GetAdjacencies();
            foreach (RoomAdjacency adj in adjacencies)
            {
                if (!roomsThatShouldBeDrawn.Contains(adj.room)) roomsThatShouldBeDrawn.Add(adj.room);
                //// go one more level of adjacency deeper because we only put triggers in cooridors
                //List<RoomAdjacency> secondLevelAdjacencies = adj.room.GetAdjacencies();
                //foreach (RoomAdjacency adj2 in secondLevelAdjacencies)
                //    if (!roomsThatShouldBeDrawn.Contains(adj2.room)) roomsThatShouldBeDrawn.Add(adj2.room);
            }

            List<Teleporter> teleporters = thisRoom.GetTeleporters();
            if (teleporters != null && teleporters.Count > 0)
            {
                foreach (Teleporter t in teleporters)
                {
                    if (!roomsThatShouldBeDrawn.Contains(t.destinationRoom)) roomsThatShouldBeDrawn.Add(t.destinationRoom);
                }
            }

            foreach (KeyValuePair<int, Room> pair in MainMap.GetCurrentLevel().rooms)
            {
                Room r = pair.Value;
                if (roomsThatShouldBeDrawn.Contains(r))
                {
                    if (!r.IsDrawn()) r.DrawRoom();
                    //if (!r.IsRendered()) r.RenderRoomObjects(true);
                    
                
                    //if (r.id != _lastTriggeredRoom)
                    //{
                    //    r.RenderRoomObjects(false);
                    //    r.RenderDoors(false);
                    //}
                }
                else
                {
                    //if (r.IsDrawn()) r.EraseRoom();
                    if(r.IsRendered()) r.RenderRoomObjects(false);
                    //r.RenderDoors(false);
                }
            }
            
        }
        System.Text.StringBuilder debugText = new System.Text.StringBuilder();
        debugText.AppendLine(string.Format("RNG seed: {0}", RNG.GetSeed()));
        debugText.AppendLine(string.Format("Current level: {0}", MainMap.GetCurrentLevel().levelNumber));
        debugText.AppendLine(string.Format("Current room: {0}", MainMap.GetCurrentRoom().id));
        GlobalUIElements.debugText.text = debugText.ToString();
    }
}
