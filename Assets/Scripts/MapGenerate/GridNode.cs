using UnityEngine;

[System.Serializable]
public class GridNode
{
    public enum Grid {
        Empty,
        Floor,
        Path, 
    }

    public Grid nodeID;
    public Vector2Int nodePos;
    public bool hasStackOn;

    public GridNode(Vector2Int nodePos, Grid nodeID) {

        this.nodePos = nodePos;
        this.nodeID = nodeID;

        hasStackOn = false;
    }

}
