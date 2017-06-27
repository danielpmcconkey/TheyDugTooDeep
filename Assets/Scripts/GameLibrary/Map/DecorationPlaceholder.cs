using UnityEngine;

namespace GameLibrary.Map
{
    public enum DecorationType { TELEPORTER }
    public class DecorationPlaceholder
    {
        public GameObject gameObject;
        public Vector3 position;
        public DecorationType type;
        public string name;
    }
}