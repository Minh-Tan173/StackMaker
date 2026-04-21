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

    public GameInput.Direct GetOtherDir(GameInput.Direct playerDir) {

        switch (playerDir) {

            case GameInput.Direct.Forward:

                if (curCornerType == CornerType.LeftDown) { return GameInput.Direct.Right; }
                if (curCornerType == CornerType.LeftUp) { return GameInput.Direct.Left; }

                break;

            case GameInput.Direct.Back:

                if (curCornerType == CornerType.RightDown) { return GameInput.Direct.Right; }
                if (curCornerType == CornerType.RightUp) { return GameInput.Direct.Left; }

                break;

            case GameInput.Direct.Right:

                if (curCornerType == CornerType.RightUp) { return GameInput.Direct.Forward; }
                if (curCornerType == CornerType.LeftUp) { return GameInput.Direct.Back; }

                break;

            case GameInput.Direct.Left:

                if (curCornerType == CornerType.RightDown) { return GameInput.Direct.Forward; }
                if (curCornerType == CornerType.LeftDown) { return GameInput.Direct.Back; }

                break;
        }

        Debug.LogError("Corner dont match with any player direction");
        return GameInput.Direct.Default;
    }
}
