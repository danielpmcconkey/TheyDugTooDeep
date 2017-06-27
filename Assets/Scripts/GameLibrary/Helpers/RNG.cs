using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class RNG : MonoBehaviour {

//	// Use this for initialization
//	void Start () {

//	}

//	// Update is called once per frame
//	void Update () {

//	}
//}
namespace GameLibrary.Helpers
{
    public static class RNG
    {
        private static int _seed;
        private static bool _seedSet = false;
        private static int _increment = 1; // don't let this ever be 0 to avoid a div/0
        private const int RESETCOUTERAT = 1000; // after how many RNG calls do you reset _increment back to 1

        public static void SetSeed(int newSeed)
        {
            _seed = newSeed;
            _seedSet = true;

        }
        public static void SetSeed()
        {
            System.Random rnd = new System.Random();
            _seed = rnd.Next(1, System.Int32.MaxValue);
            _seedSet = true;

        }
        public static int GetSeed()
        {
            return _seed;
        }
        public static int getRandomInt(int min, int max)
        {
            if (!_seedSet) return 0;
            System.Random rnd = new System.Random(_seed / _increment++);
            if (_increment > RESETCOUTERAT) _increment = 1;
            return rnd.Next(min, max + 1); // Random.Next uses an exclusive upper bound, so add one to max
        }


    }
}
