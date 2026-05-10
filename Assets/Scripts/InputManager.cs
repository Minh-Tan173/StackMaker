using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public enum Direct {
        Default,
        Forward,
        Right,
        Back,
        Left
    }

    public static InputManager Instance { get; private set; }

    public event EventHandler<OnMovedCommandEventArgs> OnMovedCommand;
    public class OnMovedCommandEventArgs : EventArgs {
        public Direct moveDirect;
    }

    [SerializeField] private float thressHold;

    private PlayerInputAction inputActions;

    private Direct moveDirect;
    private Vector2 pressedPos;
    private Vector2 realsedPos;


    private void Awake() {

        Instance = this;

        inputActions = new PlayerInputAction();

        // Mouse Actions map
        inputActions.Mouse.Enable();

        inputActions.Mouse.MousePress.started += MousePress_started;
        inputActions.Mouse.MousePress.canceled += MousePress_canceled;

        // Touce Actions map
        inputActions.Touch.Enable();

        inputActions.Touch.TouchPress.started += TouchPress_started;
        inputActions.Touch.TouchPress.canceled += TouchPress_canceled;

#if UNITY_EDITOR
        // Testing
        inputActions.Testing.Enable();
        inputActions.Testing.ResetLevel.performed += ResetLevel_performed;
#endif

    }
    private void OnDestroy() {

        inputActions.Mouse.MousePress.started -= MousePress_started;
        inputActions.Mouse.MousePress.canceled -= MousePress_canceled;

        inputActions.Touch.TouchPress.started -= TouchPress_started;
        inputActions.Touch.TouchPress.canceled -= TouchPress_canceled;

        inputActions.Testing.ResetLevel.performed -= ResetLevel_performed;
    }

#if UNITY_EDITOR
    private void ResetLevel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        // ONLY USING IN TESTING

        LevelManager.Instance.SetLevelIndex(0);

        LevelManager.Instance.OnRetry(LevelManager.Instance.OnPlay);
    }
#endif

    private void TouchPress_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        realsedPos = GetMouseScreenPos();

        float sqrMoveDis = (pressedPos - realsedPos).sqrMagnitude;
        if (sqrMoveDis <= thressHold * thressHold) {
            // If just accidentally touched the screen
            return;
        }

        OnMovedCommand?.Invoke(this, new OnMovedCommandEventArgs { moveDirect = GetDirection(pressedPos, realsedPos) });

    }

    private void TouchPress_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        pressedPos = GetMouseScreenPos();
    }

    private void MousePress_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if (LevelManager.Instance.IsEditorScene() && LevelEditorUI.Instance.IsShow()) { return; }
        if (LevelManager.Instance.GetCurrentLevelState() != LevelManager.LevelState.GameRunning) { return; }

        realsedPos = GetMouseScreenPos();

        float sqrMoveDis = (pressedPos - realsedPos).sqrMagnitude;
        if (sqrMoveDis <= thressHold * thressHold) {
            // If just accidentally touched the screen
            return;
        }


        OnMovedCommand?.Invoke(this, new OnMovedCommandEventArgs { moveDirect = GetDirection(pressedPos, realsedPos) });
    }

    private void MousePress_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if (LevelManager.Instance.IsEditorScene() && LevelEditorUI.Instance.IsShow()) { return; }
        if (LevelManager.Instance.GetCurrentLevelState() != LevelManager.LevelState.GameRunning) { return; }

        pressedPos = GetMouseScreenPos();
    }
        
    private Vector2 GetMouseScreenPos() {

        return inputActions.Mouse.MousePosition.ReadValue<Vector2>();
    }

    private Direct GetDirection(Vector2 startPos, Vector2 endPos) {

        Vector2 moveDir = endPos - startPos;

        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
            // Đi ngang

            if (moveDir.x < 0f) {
                // Sang trái

                return Direct.Left;
            }
            
            if (moveDir.x > 0f) {
                // Sang phải

                return Direct.Right;
            }

        }
        
        if (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x)) {
            // Đi dọc

            if (moveDir.y < 0f) {
                // Đi lùi

                return Direct.Back;
            }

            if (moveDir.y > 0f) {
                // Đi tiến

                return Direct.Forward;
            }
        }

        return Direct.Default;
    }

}
