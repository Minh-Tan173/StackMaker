using System;
using UnityEngine;

public class Corner : MonoBehaviour
{
    public enum CornerType {

        RightDown,
        LeftDown,
        RightUp,
        LeftUp

    }

    [Header("Visual animator")]
    [SerializeField] private Animator animator;
    
    [Header("Parent")]
    [SerializeField] private Platform platform;

    [Header("Quarternion")]
    [SerializeField] private Vector3 rightDownEuler;
    [SerializeField] private Vector3 leftDownEuler;
    [SerializeField] private Vector3 rightUpEuler;
    [SerializeField] private Vector3 leftUpEuler;

    private const string TRIGGER_INTERACT = "Interact";

    private CornerType curCornerType;

    private void Awake() {

        //Hide();

        platform.OnHideCorner += Platform_OnHideCorner; ;
        platform.OnShowCorner += PlatformVisual_OnShowCorner;

    }

    private void OnDestroy() {

        Hide();
    }

    private void OnTriggerEnter(Collider other) {
        
        if (other.CompareTag(GameTag.PLAYER_TAG)) {
            // if Trigger by Player

            animator.SetTrigger(TRIGGER_INTERACT);
        }
    }

    private void Platform_OnHideCorner(object sender, EventArgs e) {

        Hide();
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

    public InputManager.Direct GetOtherDir(InputManager.Direct playerDir) {

        switch (playerDir) {

            case InputManager.Direct.Forward:

                if (curCornerType == CornerType.LeftDown) { return InputManager.Direct.Right; }
                if (curCornerType == CornerType.LeftUp) { return InputManager.Direct.Left; }

                break;

            case InputManager.Direct.Back:

                if (curCornerType == CornerType.RightDown) { return InputManager.Direct.Right; }
                if (curCornerType == CornerType.RightUp) { return InputManager.Direct.Left; }

                break;

            case InputManager.Direct.Right:

                if (curCornerType == CornerType.RightUp) { return InputManager.Direct.Forward; }
                if (curCornerType == CornerType.LeftUp) { return InputManager.Direct.Back; }

                break;

            case InputManager.Direct.Left:

                if (curCornerType == CornerType.RightDown) { return InputManager.Direct.Forward; }
                if (curCornerType == CornerType.LeftDown) { return InputManager.Direct.Back; }

                break;
        }

        Debug.LogError("Corner dont match with any player direction");
        return InputManager.Direct.Default;
    }
}
