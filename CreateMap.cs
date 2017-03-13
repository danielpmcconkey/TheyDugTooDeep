using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLibrary.Helpers;
using GameLibrary.Map;


public class CreateMap : MonoBehaviour {

    public GameObject simpleRoundedBrick;


    public GameObject tile001;
    public float tile001Width = 0.5f;      // distance along the x axis
    public float tile001Depth = 0.5f;      // distance along the z axis
    public float tile001Height = 0.03f;      // distance along the y axis
    public float tile001ColorR = 148f;
    public float tile001ColorG = 154f;
    public float tile001ColorB = 176f;

    public GameObject brick001;
    public float brick001Width = 0.15f;      // distance along the x axis
    public float brick001Depth = 0.3f;      // distance along the z axis
    public float brick001Height = 0.15f;      // distance along the y axis
    public float brick001ColorR = 148f;
    public float brick001ColorG = 154f;
    public float brick001ColorB = 176f;

    public GameObject halfBrick001;
    public GameObject column001;


    // Use this for initialization
    void Start () {
        //RNG.SetSeed(8647211);
        //RNG.SetSeed(4217468);
        RNG.SetSeed(2821282);
        //RNG.SetSeed(7562148);
        //RNG.SetSeed(2411802);
        //RNG.SetSeed(8675309);
        setGlobalBuildingMaterials();









        MainMap mainMap = new MainMap();
        
        // level 1
        Level level1 = new Level(new Vector3(0, 0, 0));
        level1.CreateRooms();
        if(!mainMap.AddLevel(1, level1)) Debug.LogError(string.Format("Unable to add level {0}.", 1));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void setGlobalBuildingMaterials()
    {
        GlobalBuildingMaterials.tile001 = tile001;
        GlobalBuildingMaterials.tile001Width = tile001Width;
        GlobalBuildingMaterials.tile001Depth = tile001Depth;
        GlobalBuildingMaterials.tile001Height = tile001Height;
        GlobalBuildingMaterials.tile001ColorR = tile001ColorR;
        GlobalBuildingMaterials.tile001ColorG = tile001ColorG;
        GlobalBuildingMaterials.tile001ColorB = tile001ColorB;

        GlobalBuildingMaterials.brick001 = brick001;
        GlobalBuildingMaterials.brick001Width = brick001Width;
        GlobalBuildingMaterials.brick001Depth = brick001Depth;
        GlobalBuildingMaterials.brick001Height = brick001Height;
        GlobalBuildingMaterials.brick001ColorR = brick001ColorR;
        GlobalBuildingMaterials.brick001ColorG = brick001ColorG;
        GlobalBuildingMaterials.brick001ColorB = brick001ColorB;

        GlobalBuildingMaterials.halfBrick001 = halfBrick001;
        GlobalBuildingMaterials.column001 = column001;

    }
}
