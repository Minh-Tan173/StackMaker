using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public enum SceneType {
        GameScene,
        EditorScene
    };

    public enum LevelState {
        StartGame,
        GameRunning,
        WinGame
    }

    public event Action<LevelSO> LoadNewMap;

    public event EventHandler OnWinState;
    public event EventHandler InitObjectData;
    public event EventHandler ClearObjectData;
    public event EventHandler OnGameSetting;
    public event EventHandler OffGameSetting;
    public event EventHandler OnWinUI;

    [Header("Scene Type")]
    [SerializeField] private SceneType sceneType;

    [Header("Level Data")]
    [SerializeField] private LevelManagerSO levelManagerSO;

    private int currentLevelIndex;
    private LevelState currentLevelState;

    private bool isToggleSettingUI = false;

    private void Awake() {

        Instance = this;

        this.currentLevelIndex = DataManager.GetSavedLevel();
    }

    private void Start() {

        LoadLevel(this.currentLevelIndex);
        OnInit();
    }

    private IEnumerator WinningCoroutine() {

        // Update Level data first

        if (currentLevelIndex >= levelManagerSO.levelSOList.Count - 1) {
            // Reached last level --> Restart at level 1

            currentLevelIndex = 0;
        }
        else {
            this.currentLevelIndex += 1;
        }

        DataManager.SaveLevel(currentLevelIndex);

        float nextLevelTimer = 1.5f;
        yield return new WaitForSeconds(nextLevelTimer);

        // Start CloseTransition
        Transition.Instance.CloseTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        // After CloseTranstion - Reset Data and Prepared Data for next level

        LoadLevel(currentLevelIndex);
        OnInit();

        yield return new WaitForSeconds(0.3f);

        // Start OpenTransition
        Transition.Instance.OpenTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        OnPlay();
    }

    private IEnumerator RetryCoroutine(Action actionCallBack) {

        OnDespawn();

        // Start CloseTransition
        Transition.Instance.CloseTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        // After CloseTransition
        LoadLevel(this.currentLevelIndex);
        OnInit();

        yield return new WaitForSeconds(0.3f);

        // Start OpenTransition
        Transition.Instance.OpenTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        actionCallBack?.Invoke();
    }

    private void ChangeLevelStateTo(LevelState levelState) {
        this.currentLevelState = levelState;
    }

    public void OnInit() {

        ChangeLevelStateTo(LevelState.StartGame);

        InitObjectData?.Invoke(this, EventArgs.Empty);

    }

    private void OnDespawn() {

        ClearObjectData?.Invoke(this, EventArgs.Empty);
    }

    public void LoadLevel(int levelIndex) {

        this.currentLevelIndex = levelIndex;

        LoadNewMap?.Invoke(levelManagerSO.levelSOList[this.currentLevelIndex]);

    }

    public void LoadLevel(LevelSO levelSO) {

        LoadNewMap?.Invoke(levelSO);
    }

    public void OnPlay() {

        ChangeLevelStateTo(LevelState.GameRunning);
    }

    public void OnWin(bool isShowWinUI = false) {

        if (currentLevelState != LevelState.WinGame) {

            ChangeLevelStateTo(LevelState.WinGame);
            OnWinState?.Invoke(this, EventArgs.Empty);
        }

        // If in Editor Scene
        if (IsEditorScene()) { return; }

        if (!isShowWinUI) {

            OnWinUI?.Invoke(this, EventArgs.Empty);
        }
        else {

            StartCoroutine(WinningCoroutine());
        }
    }

    public void OnRetry(Action actionCallBack) {

        StartCoroutine(RetryCoroutine(actionCallBack));
    }

    public LevelState GetCurrentLevelState() {
        return this.currentLevelState;
    }

    public bool IsEditorScene() {
        return this.sceneType == SceneType.EditorScene;
    }

    public int GetCurrentLevelIndex() {
        return this.currentLevelIndex;
    }

    public LevelManagerSO GetLevelManagerSO() {
        return this.levelManagerSO;
    }

    public void ToggleGameSetting() {

        isToggleSettingUI = !isToggleSettingUI;

        if (isToggleSettingUI) {

            Time.timeScale = 0f;
            OnGameSetting?.Invoke(this, EventArgs.Empty);
        }
        else {

            Time.timeScale = 1f;
            OffGameSetting?.Invoke(this, EventArgs.Empty);
        }
    }
}
