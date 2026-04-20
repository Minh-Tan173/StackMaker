using UnityEngine;

public class PlatformVisual : MonoBehaviour
{
    public enum CornerType {

        RightDown,
        LeftDown,
        RightUp,
        LeftUp

    }

    [Header("Child Visual")]
    [SerializeField] private Transform floorVisual;
    [SerializeField] private Transform stackVisual;
    [SerializeField] private Transform cornerVisual;

    [Header("Corner Quaternion")]
    [SerializeField] private Vector3 rightDownEuler;
    [SerializeField] private Vector3 leftDownEuler;
    [SerializeField] private Vector3 rightUpEuler;
    [SerializeField] private Vector3 leftUpEuler;

    private void Awake() {

        // After Spawn
        OnInit();
    }

    private void OnInit() {

        floorVisual.gameObject.SetActive(false);
        stackVisual.gameObject.SetActive(false);
        cornerVisual.gameObject.SetActive(false);
    }

    public void ShowFloor() {

        floorVisual.gameObject.SetActive(true);
        stackVisual.gameObject.SetActive(false);
        cornerVisual.gameObject.SetActive(false);
    }

    public void ShowStack() {

        stackVisual.gameObject.SetActive(true);
        floorVisual.gameObject.SetActive(false);
        cornerVisual.gameObject.SetActive(false);
    }

    public void ShowCorner(CornerType cornerType) {

        if (cornerType == CornerType.RightDown) {

            cornerVisual.transform.localRotation = Quaternion.Euler(rightDownEuler);
        }
        else if (cornerType == CornerType.LeftDown) {

            cornerVisual.transform.localRotation = Quaternion.Euler(leftDownEuler);
        }
        else if (cornerType == CornerType.RightUp) {

            cornerVisual.transform.localRotation = Quaternion.Euler(rightUpEuler);
        }
        else if (cornerType == CornerType.LeftUp) {

            cornerVisual.transform.localRotation = Quaternion.Euler(leftUpEuler);
        }

        cornerVisual.gameObject.SetActive(true);
    }

    public void HideStack() {
        if (stackVisual.gameObject.activeSelf) {

            stackVisual.gameObject.SetActive(false);
        }
    }

}
