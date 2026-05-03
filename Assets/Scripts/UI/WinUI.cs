using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WinUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI winNotiText;

    [Header("Button")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Pos")]
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 endPos;

    private RectTransform rectTransform;

    private void Awake() {

        rectTransform = GetComponent<RectTransform>();

        nextLevelButton.onClick.AddListener(() => {

            LevelManager.Instance.OnWin(isShowWinUI: true);

            OnDespawn();
            StartCoroutine(HideCoroutine(null));
        });

        retryButton.onClick.AddListener(() => {

            StartCoroutine(HideCoroutine(() => {

                LevelManager.Instance.OnRetry(actionCallBack: LevelManager.Instance.OnPlay);
            }));

        });

        mainMenuButton.onClick.AddListener(() => {

            StartCoroutine(HideCoroutine(() => {

                LevelManager.Instance.OnRetry(actionCallBack: MainMenuUI.Instance.Show);
            }));
        });
    }

    private void Start() {

        LevelManager.Instance.OnWinUI += LevelManager_OnWinUI;

        OnInit();
    }

    private void OnDestroy() {
        LevelManager.Instance.OnWinUI -= LevelManager_OnWinUI;
    }

    private void LevelManager_OnWinUI(object sender, System.EventArgs e) {

        Show();
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine() {

        float elapsed = 0f;
        float duration = 0.5f;

        rectTransform.anchoredPosition = startPos;

        while (elapsed <= duration) {

            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            rectTransform.anchoredPosition = Vector3.LerpUnclamped(startPos, endPos, EaseOutBack(t));

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
    }

    private IEnumerator HideCoroutine(Action actionCallBack) {

        float elapsed = 0f;
        float duration = 0.2f;

        rectTransform.anchoredPosition = endPos;

        while (elapsed <= duration) {

            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            rectTransform.anchoredPosition = Vector3.LerpUnclamped(endPos, startPos, EaseInBack(t));

            yield return null;
        }

        rectTransform.anchoredPosition = startPos;

        actionCallBack?.Invoke();

        Hide();
    }

    private void OnInit() {

        rectTransform.anchoredPosition = startPos;
        Hide();

        int currentLevelIndex = LevelManager.Instance.GetCurrentLevelIndex();

        // Update Text
        int currentLevel = currentLevelIndex + 1;
        winNotiText.text = $"Win Level {currentLevel}";

        // Setup NextLevelButton
        int lastLevelIndex = LevelManager.Instance.GetLevelManagerSO().levelSOList.Count - 1;
        if (currentLevelIndex < lastLevelIndex) {

            ShowNextLevelButton();
        }
        else {
            HideNextLevelButton();
        }
    }

    private void OnDespawn() {
        retryButton.GetComponent<Animator>().Rebind();
        nextLevelButton.GetComponent<Animator>().Rebind();
        mainMenuButton.GetComponent<Animator>().Rebind();
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }

    private void ShowNextLevelButton() {
        this.gameObject.SetActive(true);
    }

    private void HideNextLevelButton() {
        nextLevelButton.gameObject.SetActive(false);
    }

    private float EaseOutBack(float t) {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
    }

    private float EaseInBack(float t) {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return c3 * t * t * t - c1 * t * t;
    }
}
