using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public static SettingUI Instance { get; private set; }

    [Header("Button")]
    [SerializeField] private Button sfxButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button closeButton;

    [Header("Pos")]
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 endPos;

    private RectTransform rectTransform;
    private bool isRunningCoroutine = false;

    private void Awake() {

        Instance = this;

        rectTransform = GetComponent<RectTransform>();

        sfxButton.onClick.AddListener(() => {

            SoundManager.Instance.ToggleSFXVolume();
        });

        musicButton.onClick.AddListener(() => {

            MusicManager.Instance.ToggleMusicVolume();
        });

        mainMenuButton.onClick.AddListener(() => {

            LevelManager.Instance.OnRetry(actionCallBack: MainMenuUI.Instance.Show);
            LevelManager.Instance.ToggleGameSetting();

        });

        retryButton.onClick.AddListener(() => {

            LevelManager.Instance.OnRetry(actionCallBack: LevelManager.Instance.OnPlay);
            LevelManager.Instance.ToggleGameSetting();
        });

        closeButton.onClick.AddListener(() => {

            LevelManager.Instance.ToggleGameSetting();
        });
    }

    private void Start() {

        LevelManager.Instance.OnGameSetting += LevelManager_OnGameSetting;
        LevelManager.Instance.OffGameSetting += LevelManager_OffGameSetting;

        OnInit();
    }

    private void OnDestroy() {

        LevelManager.Instance.OnGameSetting -= LevelManager_OnGameSetting;
        LevelManager.Instance.OffGameSetting -= LevelManager_OffGameSetting;
    }

    private void LevelManager_OffGameSetting(object sender, System.EventArgs e) {

        OnDespawn();

        StartCoroutine(HideCoroutine());
    }

    private void LevelManager_OnGameSetting(object sender, System.EventArgs e) {

        Show();

        StartCoroutine(ShowCoroutine());

    }

    private IEnumerator ShowCoroutine() {

        isRunningCoroutine = true;

        float elapsed = 0f;
        float duration = 0.2f;

        rectTransform.anchoredPosition = startPos;

        while (elapsed <= duration) {

            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            rectTransform.anchoredPosition = Vector3.LerpUnclamped(startPos, endPos, EaseOutBack(t));

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;

        isRunningCoroutine = false;
    }

    private IEnumerator HideCoroutine() {

        isRunningCoroutine = true;

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

        isRunningCoroutine = false;

        Hide();
    }

    private void OnInit() {

        // Update Pos
        rectTransform.anchoredPosition = startPos;

        // Update Sound Button
        if (SoundManager.Instance.IsMutedSFX()) {

            sfxButton.GetComponent<SpriteSwapButton>().SwapSprite();
        }
        
        if (MusicManager.Instance.IsMutedMusic()) {

            musicButton.GetComponent<SpriteSwapButton>().SwapSprite();
        }

        Hide(); 
       
    }

    private void OnDespawn() {
        retryButton.GetComponent<Animator>().Rebind();
        mainMenuButton.GetComponent<Animator>().Rebind();
    }

    private void Show() {

        this.gameObject.SetActive(true);
    }

    private void Hide() {

        this.gameObject.SetActive(false);
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

    public bool IsRunningCoroutine() {
        return this.isRunningCoroutine;
    }
}
