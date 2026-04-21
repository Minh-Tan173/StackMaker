using System;
using UnityEngine;

public class Platform : MonoBehaviour
{

    public event EventHandler<ShowCornerEventArgs> OnShowCorner;
    public class ShowCornerEventArgs : EventArgs {
        public Corner.CornerType cornerType;
    }

    [Header("Child Visual")]
    [SerializeField] private Transform floorVisual;
    [SerializeField] private Transform stackVisual;

    private GridNode.NodeID nodeID;

    private void Awake() {

        // After Spawn
        OnInit();
    }

    private void OnInit() {

        floorVisual.gameObject.SetActive(false);
        stackVisual.gameObject.SetActive(false);
    }

    private void ChangeNodeIDTo(GridNode.NodeID nodeID) {
        this.nodeID = nodeID;
    }

    public void ShowFloor() {

        ChangeNodeIDTo(GridNode.NodeID.Floor);

        floorVisual.gameObject.SetActive(true);
        stackVisual.gameObject.SetActive(false);
    }

    public void ShowStack() {

        ChangeNodeIDTo(GridNode.NodeID.Path);

        stackVisual.gameObject.SetActive(true);
        floorVisual.gameObject.SetActive(false);
    }
        
    public void ShowCorner(Corner.CornerType cornerType) {

        OnShowCorner?.Invoke(this, new ShowCornerEventArgs { cornerType = cornerType });
    }

    public void HideStack() {
        if (stackVisual.gameObject.activeSelf) {

            stackVisual.gameObject.SetActive(false);
        }
    }

    public GridNode.NodeID GetNodeID() {
        return this.nodeID;
    }
}
