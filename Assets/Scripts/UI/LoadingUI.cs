using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image loadingProgress;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private List<string> loadingTextList;

    private const string GAME_SCENE = "GameScene";

    private void Start() {

        OnInit();
    }

    private void OnInit() {

        StartCoroutine(LoadingTextCoroutine());
        StartCoroutine(LoadingSceneCoroutine());

    }


    private IEnumerator LoadingSceneCoroutine() {

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(GAME_SCENE);

        loadOperation.allowSceneActivation = false;

        float visualProgress = 0f;
        SetLoadProgress(visualProgress);

        while (loadOperation.progress < 1f) {

            float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);

            float loadSpeed = Time.deltaTime * 0.9f;

            visualProgress = Mathf.MoveTowards(visualProgress, progress, loadSpeed);

            SetLoadProgress(visualProgress);

            if (visualProgress >= 1f) {
                loadOperation.allowSceneActivation = true;
            }

            yield return null;
        }

    }

    private IEnumerator LoadingTextCoroutine() {

        float waitTimer = 0.2f;
        int currentIndex = 0;
        int totalIndex = loadingTextList.Count;

        loadingText.text = loadingTextList[currentIndex];

        while (true) {

            currentIndex = (currentIndex + 1) % totalIndex;
            loadingText.text = loadingTextList[currentIndex];

            yield return new WaitForSeconds(waitTimer);
        }

    }

    private void SetLoadProgress(float amount) {
        loadingProgress.fillAmount = amount;
    }
}
