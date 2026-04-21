using UnityEngine;

public class WinPos : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;

    public Transform GetTargetPoint() {
        return this.targetPoint;
    }
}
