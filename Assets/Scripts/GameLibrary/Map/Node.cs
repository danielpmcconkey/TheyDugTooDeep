using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameLibrary.Map
{
    public class Node : GameLibrary.AI.IHeapItem<Node>
    {
        public bool isWalkable;
        public Vector3 worldPosition;
        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }
        public int gridX;
        public int gridZ;
        public Node parent;
        private int _heapIndex;
        public int HeapIndex { get { return _heapIndex; } set { _heapIndex = value; } }

        public Node(bool walkable, Vector3 position, int pGridX, int pGridZ)
        {
            isWalkable = walkable;
            worldPosition = position;
            gridX = pGridX;
            gridZ = pGridZ;
        }
        public int CompareTo(Node n)
        {
            int compare = fCost.CompareTo(n.fCost);
            if(compare == 0)
            {
                compare = hCost.CompareTo(n.hCost);
            }
            return -compare;
        }
        
    }
}
