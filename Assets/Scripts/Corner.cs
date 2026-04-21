using UnityEngine;

public class Corner : MonoBehaviour
{
    public enum CornerType {

        RightDown,
        LeftDown,
        RightUp,
        LeftUp

    }

    [Header("Parent")]
    [SerializeField] private Platform platform;

    [Header("Quarternion")]
    [SerializeField] private Vector3 rightDownEuler;
    [SerializeField] private Vector3 leftDownEuler;
    [SerializeField] private Vector3 rightUpEuler;
    [SerializeField] private Vector3 leftUpEuler;

    private CornerType curCornerType;

    private void Awake() {

        Hide();

        platform.OnShowCorner += PlatformVisual_OnShowCorner;
    }

    private void PlatformVisual_OnShowCorner(object sender, Platform.ShowCornerEventArgs e) {

        ShowCorner(e.cornerType);
    }

    public void ShowCorner(Corner.CornerType cornerType) {

        this.curCornerType = cornerType;

        if (cornerType == Corner.CornerType.RightDown) {

            this.transform.localRotation = Quaternion.Euler(rightDownEuler);
        }
        else if (cornerType == Corner.CornerType.LeftDown) {

            this.transform.localRotation = Quaternion.Euler(leftDownEuler);
        }
        else if (cornerType == Corner.CornerType.RightUp) {

            this.transform.localRotation = Quaternion.Euler(rightUpEuler);
        }
        else if (cornerType == Corner.CornerType.LeftUp) {

            this.transform.localRotation = Quaternion.Euler(leftUpEuler);
        }

        Show();
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }

    public void GetOtherDir() {

       
    }
}
