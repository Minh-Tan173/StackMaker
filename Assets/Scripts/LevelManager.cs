using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event EventHandler<LoadNewMapEventArgs> LoadNewMap;
    public class LoadNewMapEventArgs : EventArgs {
        public LevelSO levelSO;
    }
    public event EventHandler InitObjectData;

    [Header("Level Data")]
    [SerializeField] private LevelManagerSO levelManagerSO;

    private int currentLevelIndex;

    public enum LevelState {
        StartGame,
        GameRunning,
        WinGame
    }

    public event EventHandler OnWinState;

    private LevelState currentLevelState;

    private void Awake() {

        Instance = this;
    }

    private void Start() {

        LoadLevel(0);
        OnInit();
    }

    private IEnumerator WinningCoroutine() {

        ChangeLevelStateTo(LevelState.WinGame);

        OnWinState?.Invoke(this, EventArgs.Empty);

        float nextLevelTimer = 4f;
        yield return new WaitForSeconds(nextLevelTimer);

        // Start CloseTransition
        Transition.Instance.CloseTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        // After CloseTranstion - Reset Data and Prepared Data for next level
        LoadLevel(this.currentLevelIndex += 1);
        OnInit();

        yield return new WaitForSeconds(0.3f);

        // Start OpenTransition
        Transition.Instance.OpenTransition();

        while (Transition.Instance.IsRunningCoroutine()) {
            yield return null;
        }

        OnPlay();
    }

    private void ChangeLevelStateTo(LevelState levelState) {
        this.currentLevelState = levelState;
    }

    private void OnInit() {

        ChangeLevelStateTo(LevelState.StartGame);

        InitObjectData?.Invoke(this, EventArgs.Empty);

    }

    private void OnDespawn() {

    }

    public void LoadLevel(int levelIndex) {

        this.currentLevelIndex = levelIndex;

        LoadNewMap?.Invoke(this, new LoadNewMapEventArgs { levelSO = levelManagerSO.levelSOArray[this.currentLevelIndex] });
    }

    public void OnPlay() {

        ChangeLevelStateTo(LevelState.GameRunning);
    }

    public void OnWin() {

        StartCoroutine(WinningCoroutine());
    }

    public void OnRetry() {

        OnDespawn();

        LoadLevel(this.currentLevelIndex);

        OnInit();
    }

    public LevelState GetCurrentLevelState() {
        return this.currentLevelState;
    }

    public int GetCurrentLevelIndex() {
        return this.currentLevelIndex;
    }
}
