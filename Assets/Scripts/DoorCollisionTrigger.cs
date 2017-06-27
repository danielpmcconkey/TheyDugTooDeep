using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Map;

public class DoorCollisionTrigger : MonoBehaviour {

    public float doorSmooth = 2.0f;
    public float doorOpenAngle = 90;
    public float barSmooth = 2.0f;
    public float barOpenAngle = 90;
    public Transform leftDoor;
    public Transform rightDoor;
    public Transform leftBar;
    public Transform rightBar;

    private bool _isOpen = false;
    private bool _isEnter = false;
    private bool _isUnlocked = false;
    private Vector3 _defaultRotationLeftDoor;
    private Vector3 _openRotationLeftDoor;
    private Vector3 _defaultRotationRightDoor;
    private Vector3 _openRotationRightDoor;
    private Vector3 _defaultRotationLeftBar;
    private Vector3 _openRotationLeftBar;
    private Vector3 _defaultRotationRightBar;
    private Vector3 _openRotationRightBar;
    private int _lesserRoomId;
    private int _greaterRoomId;

    public void LockDoor()
    {
        _isUnlocked = false;
    }
    public void UnlockDoor()
    {
        _isUnlocked = true;
    }


    private void Start()
    {
        _defaultRotationLeftDoor = leftDoor.eulerAngles;
        _openRotationLeftDoor = new Vector3(_defaultRotationLeftDoor.x, _defaultRotationLeftDoor.y + doorOpenAngle, _defaultRotationLeftDoor.z);
        _defaultRotationRightDoor = rightDoor.eulerAngles;
        _openRotationRightDoor = new Vector3(_defaultRotationRightDoor.x, _defaultRotationRightDoor.y + doorOpenAngle, _defaultRotationRightDoor.z);

        _defaultRotationLeftBar = leftBar.eulerAngles;
        _openRotationLeftBar = new Vector3(_defaultRotationLeftBar.x, _defaultRotationLeftBar.y, _defaultRotationLeftBar.z + barOpenAngle);
        _defaultRotationRightBar = rightBar.eulerAngles;
        _openRotationRightBar = new Vector3(_defaultRotationRightBar.x, _defaultRotationRightBar.y, _defaultRotationRightBar.z + barOpenAngle);

        string doorName = transform.name;
        //Debug.Log(string.Format("Door name =", doorName));
        string[] nameSplit = doorName.Split(':');
        _lesserRoomId = int.Parse(nameSplit[1]);
        _greaterRoomId = int.Parse(nameSplit[2]);

        //SendMessage("UnlockDoor");
    }

    private void Update()
    {
        if(_isUnlocked)
        {
            // unlock door
            leftBar.eulerAngles = Vector3.Slerp(leftBar.eulerAngles, _openRotationLeftBar, Time.deltaTime * barSmooth);
            rightBar.eulerAngles = Vector3.Slerp(rightBar.eulerAngles, _openRotationRightBar, Time.deltaTime * barSmooth);
        }
        else
        {
            // lock door
            leftBar.eulerAngles = Vector3.Slerp(leftBar.eulerAngles, _defaultRotationLeftBar, Time.deltaTime * barSmooth);
            rightBar.eulerAngles = Vector3.Slerp(rightBar.eulerAngles, _defaultRotationRightBar, Time.deltaTime * barSmooth);
        }

        if (_isOpen)
        {
            // open door
            leftDoor.eulerAngles = Vector3.Slerp(leftDoor.eulerAngles, _openRotationLeftDoor, Time.deltaTime * doorSmooth);
            rightDoor.eulerAngles = Vector3.Slerp(rightDoor.eulerAngles, _openRotationRightDoor, Time.deltaTime * doorSmooth);
        }
        else
        {
            // close door
            leftDoor.eulerAngles = Vector3.Slerp(leftDoor.eulerAngles, _defaultRotationLeftDoor, Time.deltaTime * doorSmooth);
            rightDoor.eulerAngles = Vector3.Slerp(rightDoor.eulerAngles, _defaultRotationRightDoor, Time.deltaTime * doorSmooth);
        }

        //if (Input.GetKeyDown("f") && _isEnter)
        //{
        //    _isOpen = !_isOpen;
        //}
    }

    //private void OnGUI()
    //{
    //    if(_isEnter)
    //    {
    //        GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, 150, 30), "Press \"F\" to open the door");
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Debug.Log(string.Format("Door collision with {0}", other.gameObject.tag));
            _isEnter = true;
            if(_isUnlocked && !_isOpen) _isOpen = true;

            // make sure both rooms are rendered
            Level currentLevel = MainMap.GetCurrentLevel();
            Room lesserRoom = currentLevel.rooms[_lesserRoomId];
            if (!lesserRoom.IsRendered()) lesserRoom.RenderRoomObjects(true);
            Room greaterRoom = currentLevel.rooms[_greaterRoomId];
            if (!greaterRoom.IsRendered()) greaterRoom.RenderRoomObjects(true);

            Room currentRoom = MainMap.GetCurrentRoom();
            if (lesserRoom != currentRoom && _isUnlocked) lesserRoom.FadeTorches();
            if (greaterRoom != currentRoom && _isUnlocked) greaterRoom.FadeTorches();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //Debug.Log(string.Format("Door collision exit with {0}", other.gameObject.tag));
            _isEnter = false;
            if(_isOpen) _isOpen = false;
        }
    }
}
