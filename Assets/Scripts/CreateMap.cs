﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;
using GameLibrary.Map;


public class CreateMap : MonoBehaviour {

    #region tilesets
    [Header("Tile sets")]
    public GameObject tile001;
    public float tile001Width = 0.5f;      // distance along the x axis
    public float tile001Depth = 0.5f;      // distance along the z axis
    public float tile001Height = 0.03f;      // distance along the y axis
    public GameObject brick001;
    public float brick001Width = 0.15f;      // distance along the x axis
    public float brick001Depth = 0.3f;      // distance along the z axis
    public float brick001Height = 0.15f;      // distance along the y axis
    public GameObject halfBrick001;
    public GameObject column001;
    public GameObject torch001;
    public GameObject roomEntryTrigger;
    public GameObject door001;
    public GameObject pentagram;
    #endregion tilesets

    #region startup
    [Header("Start up settings")]
    public Color portalRoomColor = new Color(0.1f, 0.1f, 0.1f);
    public UnityEngine.UI.Text seedTextDisplay;
    public UnityEngine.UI.InputField seedOverwriteInput;
    public UnityEngine.UI.Text mapProgressText;
    #endregion startup


    #region map parameters
    [Header("Map parameters")]
    public int numFloors = 6;
    public int mapSize = 50; // the max distance from the center point in all directions
    public int minRoomsPerFloor = 10;
    public int maxRoomsPerFloor = 12;
    public int minRoomSize = 8;
    public int maxRoomSize = 25;
    public int startingRoomRadius = 7;
    public int roomHeight = 2;
    public int corridorWidth = 2;
    public int rightAngleConnectorSize = 4;
    public int minCorridorLength = 3;
    public int maxCorridorLength = 30;
    public int maxRetries = 25; // used for while loops to prevent infinite looping
    public float torchWallOffset = 0.25f;
    public int colorVarianceMax = 10;
    public string unwalkableLayerName = "Unwalkable";
    #endregion map parameters

    #region characters
    public GameObject MonsterToken;
    public Transform player;
    #endregion characters

    private float _percentMapComplete = 0f;
    private string _progressText = string.Empty;
   


    // Use this for initialization
    void Start () {

        
        //RNG.SetSeed(8647211);
        //RNG.SetSeed(4217468);
        //RNG.SetSeed(2821282);
        //RNG.SetSeed(7562148);
        //RNG.SetSeed(2411802);
        //RNG.SetSeed(8675309);
        RNG.SetSeed();
        setGlobalMapParameters();
        setGlobalBuildingMaterials();


        createPortal();



    }

    public void RenderCurrentLevelStartUp()
    {
        MainMap.GetCurrentRoom().RenderRoomObjects(true);
        MainMap.GetCurrentLevel().RenderDoors(true);
    }

    public void UpdateSeed()
    {
        int newSeed = RNG.GetSeed();
        int.TryParse(seedOverwriteInput.text, out newSeed);
        RNG.SetSeed(newSeed);
        seedTextDisplay.text = string.Format("Your RNG seed is: {0}", RNG.GetSeed());
    }

    public void GenerateGameLevels()
    {
        //mapProgressText.enabled = true;
        mapProgressText.text = string.Format("{0}% complete: beginning", _percentMapComplete);

        StartCoroutine("buildLevels");
        //buildLevels();

        
       

    }

    private IEnumerator generateGameMap()
    {
        AsyncOperation async = (AsyncOperation)buildLevels();
        while(!async.isDone)
        {
            mapProgressText.text = string.Format("{0}% complete", _percentMapComplete) + System.Environment.NewLine + _progressText;
            yield return null;
        }
    }

    //public void RenderLevel(int level)
    //{
    //    for (int i = 0; i < MainMap.levels.Count; i++)
    //    {
    //        if (i == level) MainMap.levels[i].RenderLevel(true);
    //        else MainMap.levels[i].RenderLevel(false);
    //    }
    //}

    private void OnDrawGizmos()
    {
        if (MainMap.GetCurrentLevel() != null)
        {
            Room r = MainMap.GetCurrentRoom();
            if (r != null) r.DrawPathFindingGizmos();
        }
    }

    private void createPortal()
    {
        int tileSet = 1; // todo: do something about tilesets

        float northEdge = 0 + GlobalMapParameters.startingRoomRadius;
        float southEdge = 0 - GlobalMapParameters.startingRoomRadius;
        float eastEdge = 0 + GlobalMapParameters.startingRoomRadius;
        float westEdge = 0 - GlobalMapParameters.startingRoomRadius;


        Room portalRoom = new Room(0, new Vector3(westEdge, 10, southEdge), new Vector3(eastEdge, GlobalMapParameters.roomHeight + 10, northEdge), tileSet, portalRoomColor);

        Teleporter t = new Teleporter() {
            sourceRoom = portalRoom,
            source = new Vector3(4, 10, -4),
            destination = new Vector3(0, 1, 0)
        };
        DecorationPlaceholder dph = portalRoom.AddTeleporter(t);
        dph.name = string.Format("Teleporter|{0}|{1}|{2}", 0, 1, 0);

        // add torches, 2 on each wall
        float torchHeight = 11.25f;
        float torchSpacing = GlobalMapParameters.startingRoomRadius / 1.5f;
        // north wall
        portalRoom.AddTorch(new Vector3(
                (0 - GlobalMapParameters.startingRoomRadius) + torchSpacing,
                torchHeight,
                GlobalMapParameters.startingRoomRadius - GlobalMapParameters.torchWallOffset
            ));
        portalRoom.AddTorch(new Vector3(
                GlobalMapParameters.startingRoomRadius - torchSpacing,
                torchHeight,
                GlobalMapParameters.startingRoomRadius - GlobalMapParameters.torchWallOffset
            ));
        // south wall
        portalRoom.AddTorch(new Vector3(
                (0 - GlobalMapParameters.startingRoomRadius) + torchSpacing,
                torchHeight,
                0 - GlobalMapParameters.startingRoomRadius + GlobalMapParameters.torchWallOffset
            ));
        portalRoom.AddTorch(new Vector3(
                GlobalMapParameters.startingRoomRadius - torchSpacing,
                torchHeight,
                0 - GlobalMapParameters.startingRoomRadius + GlobalMapParameters.torchWallOffset
            ));
        // east wall
        portalRoom.AddTorch(new Vector3(
                GlobalMapParameters.startingRoomRadius - GlobalMapParameters.torchWallOffset,
                torchHeight,
                (0 - GlobalMapParameters.startingRoomRadius) + torchSpacing
            ));
        portalRoom.AddTorch(new Vector3(
                GlobalMapParameters.startingRoomRadius - GlobalMapParameters.torchWallOffset,
                torchHeight,
                GlobalMapParameters.startingRoomRadius - torchSpacing
            ));
        // west wall
        portalRoom.AddTorch(new Vector3(
                0 - GlobalMapParameters.startingRoomRadius + GlobalMapParameters.torchWallOffset,
                torchHeight,
                (0 - GlobalMapParameters.startingRoomRadius) + torchSpacing
            ));
        portalRoom.AddTorch(new Vector3(
                0 - GlobalMapParameters.startingRoomRadius + GlobalMapParameters.torchWallOffset,
                torchHeight,
                GlobalMapParameters.startingRoomRadius - torchSpacing
            ));

        portalRoom.DrawRoom();
        seedTextDisplay.text = string.Format("Your RNG seed is: {0}", RNG.GetSeed());
    }

    private void printProgress(string newUpdate)
    {
        _progressText = newUpdate + System.Environment.NewLine + _progressText;
        //print(_progressText);
        mapProgressText.text = string.Format("{0}% complete", _percentMapComplete) + System.Environment.NewLine + _progressText;
    }

    private IEnumerator buildLevels()
    {
        _progressText = string.Empty;
        for(int i = 0; i < GlobalMapParameters.numFloors; i++)
        {
            float percentEachLevel = Mathf.Round(96 / GlobalMapParameters.numFloors); // leave 4 % for rendering start room
            _percentMapComplete = i * percentEachLevel;
            printProgress(string.Format("Beginning level {0}", i + 1));
            yield return null;


            float percentEachStep = Mathf.Round(percentEachLevel / 4);

            Level level = new Level(new Vector3(0, 0, 0), i);
            printProgress("   Creating rooms");
            yield return null;
            level.CreateRooms();

            _percentMapComplete += percentEachStep;
            printProgress("   Decorating rooms");
            yield return null;
            level.DecorateRooms();

            _percentMapComplete += percentEachStep;
            printProgress("   Populating rooms");
            yield return null;
            level.PopulateRooms();

            _percentMapComplete += percentEachStep;
            printProgress("   Hiding level objects");
            yield return null;
            level.RenderEntireLevel(false);

            MainMap.AddLevel(level);
            _percentMapComplete += percentEachStep;
            printProgress("   Level complete");
            yield return null;
        }

        printProgress("Initializing start-up");
        yield return null;

        MainMap.SetCurrentLevelId(0);
        MainMap.SetPlayer(player);
        RenderCurrentLevelStartUp();

        _percentMapComplete = 100;
        printProgress("Game levels are complete.");

    }

    private void setGlobalMapParameters()
    {
        GlobalMapParameters.numFloors = numFloors;
        GlobalMapParameters.mapSize = mapSize;
        GlobalMapParameters.minRoomsPerFloor = minRoomsPerFloor;
        GlobalMapParameters.maxRoomsPerFloor = maxRoomsPerFloor;
        GlobalMapParameters.minRoomSize = minRoomSize;
        GlobalMapParameters.maxRoomSize = maxRoomSize;
        GlobalMapParameters.startingRoomRadius = startingRoomRadius;
        GlobalMapParameters.roomHeight = roomHeight;
        GlobalMapParameters.corridorWidth = corridorWidth;
        GlobalMapParameters.rightAngleConnectorSize = rightAngleConnectorSize;
        GlobalMapParameters.minCorridorLength = minCorridorLength;
        GlobalMapParameters.maxCorridorLength = maxCorridorLength;
        GlobalMapParameters.maxRetries = maxRetries;
        GlobalMapParameters.torchWallOffset = torchWallOffset;
        GlobalMapParameters.colorVarianceMax = colorVarianceMax;
        GlobalMapParameters.unwalkableLayerName = unwalkableLayerName;
    }
    private void setGlobalBuildingMaterials()
    {
        GlobalBuildingMaterials.roomEntryTrigger = roomEntryTrigger;

        #region tilesets
        GlobalBuildingMaterials.tile001 = tile001;
        GlobalBuildingMaterials.tile001Width = tile001Width;
        GlobalBuildingMaterials.tile001Depth = tile001Depth;
        GlobalBuildingMaterials.tile001Height = tile001Height;

        GlobalBuildingMaterials.brick001 = brick001;
        GlobalBuildingMaterials.brick001Width = brick001Width;
        GlobalBuildingMaterials.brick001Depth = brick001Depth;
        GlobalBuildingMaterials.brick001Height = brick001Height;

        GlobalBuildingMaterials.halfBrick001 = halfBrick001;
        GlobalBuildingMaterials.column001 = column001;
        GlobalBuildingMaterials.torch001 = torch001;

        GlobalBuildingMaterials.door001 = door001;
        GlobalBuildingMaterials.pentagram = pentagram;
        #endregion tilesets

        #region characters
        GlobalBuildingMaterials.MonsterToken = MonsterToken;
        #endregion characters

    }
}
