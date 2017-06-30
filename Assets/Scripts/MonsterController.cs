using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Map;
using GameLibrary.Character;
using GameLibrary.AI;

public class MonsterController : MonoBehaviour {

    public float speed = 5;
    public float pathUpdateTargetMovementThreshold = 0.5f;
    public float minPathUpdateTime = 0.2f;

    private Monster _monster;
    private Vector3[] path;
    int targetIndex;
    private Room _room;
    private Transform _target;

    // Use this for initialization
    void Start () {

        string[] nameSplit = transform.name.Split(':');
        _room = MainMap.GetCurrentLevel().rooms[int.Parse(nameSplit[1])];
        _monster = _room.GetMonster(int.Parse(nameSplit[2]));
        _target = MainMap.GetPlayer();
        StartCoroutine(UpdatePath());


    }
	
	

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        if(path!= null) foreach (Vector3 p in path) Gizmos.DrawCube(p + Vector3.up, Vector3.one);
    }

    private IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < 0.3f) yield return new WaitForSeconds(0.3f);

        PathRequest pathRequest = new PathRequest(_monster.GetPosition(), _target.position, OnPathFound, _room.GetRoomGrid(), this);
        StartCoroutine(Pathfinder.RequestPath(pathRequest));

        float sqrMoveThreshold = Mathf.Pow(pathUpdateTargetMovementThreshold, 2);
        Vector3 targetPosOld = _target.position;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((_target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                pathRequest = new PathRequest(_monster.GetPosition(), _target.position, OnPathFound, _room.GetRoomGrid(), this);
                StopCoroutine(Pathfinder.RequestPath(pathRequest));
                StartCoroutine(Pathfinder.RequestPath(pathRequest));
                targetPosOld = _target.position;
            }
        }
    }

    public void OnPathFound(Vector3[] newPath, bool isSuccessful)
    {
        if (isSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    IEnumerator FollowPath()
    {
        if (path != null && path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];
            while (true)
            {
                if (transform.position == currentWaypoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length) yield break;
                    currentWaypoint = path[targetIndex];
                }
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, step);
                yield return null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(string.Format("Collided with object. Name: {0}. Tag: {1}.", other.gameObject.name, other.gameObject.tag));
        switch (other.gameObject.tag)
        {
            case "Player":
                KillMe();
                break;
        }
    }
    private void KillMe()
    {
        if(_monster == null)
        {
            string[] nameSplit = transform.name.Split(':');
            Room r = MainMap.GetCurrentLevel().rooms[int.Parse(nameSplit[1])];
            _monster = r.GetMonster(int.Parse(nameSplit[2]));
        }
        if(_monster.IsAlive()) _monster.Kill();
    }

}
