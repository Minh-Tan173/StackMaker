using System;
using UnityEngine;

public class Platform : MonoBehaviour
{

    public event EventHandler<ShowCornerEventArgs> OnShowCorner;
    public class ShowCornerEventArgs : EventArgs {
        public Corner.CornerType cornerType;
    }

    public event EventHandler OnHideCorner;

    [Header("Child Visual")]
    [SerializeField] private Transform floorVisual;
    [SerializeField] private Transform stackVisual;

    private GridNode.NodeID nodeID;
    private bool hasCornerOn = false;
    private bool isStackVisualOn = false;

    private void ChangeNodeIDTo(GridNode.NodeID nodeID) {
        this.nodeID = nodeID;
    }

    public void ResetToDefault() {

        floorVisual.gameObject.SetActive(false);
        stackVisual.gameObject.SetActive(false);

        OnHideCorner?.Invoke(this, EventArgs.Empty);
    }

    public void ShowFloor() {

        ChangeNodeIDTo(GridNode.NodeID.Wall);

        floorVisual.gameObject.SetActive(true);
        stackVisual.gameObject.SetActive(false);
    }

    public void ShowStack() {

        ChangeNodeIDTo(GridNode.NodeID.Path);

        stackVisual.gameObject.SetActive(true);
        floorVisual.gameObject.SetActive(false);

        isStackVisualOn = true;
    }
        
    public void ShowCorner(Corner.CornerType cornerType) {

        hasCornerOn = true;

        OnShowCorner?.Invoke(this, new ShowCornerEventArgs { cornerType = cornerType });
    }

    public void HideStack() {

        if (stackVisual.gameObject.activeSelf) {

            isStackVisualOn = false;
            stackVisual.gameObject.SetActive(false);
        }
    }

    public GridNode.NodeID GetNodeID() {
        return this.nodeID;
    }

    public bool HasCornerOn() {
        return this.hasCornerOn;
    }

    public bool IsStackVisualOn() {
        return this.isStackVisualOn;
    }
}
