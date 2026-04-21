using UnityEngine;

[System.Serializable]
public class GridNode
{
    public enum NodeID {
        Empty,
        Floor,
        Path, 
    }

    public NodeID nodeID;
    public Vector2Int nodePos;
    public bool hasStackOn;

    public GridNode(Vector2Int nodePos, NodeID nodeID) {

        this.nodePos = nodePos;
        this.nodeID = nodeID;

        hasStackOn = false;
    }

}
