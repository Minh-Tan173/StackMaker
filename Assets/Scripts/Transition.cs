using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    public static Transition Instance;

    [Header("Image")]
    [SerializeField] private RectTransform downImage;
    [SerializeField] private RectTransform upImage;

    [Header("End Pos")]
    [SerializeField] private Vector3 endPosDown;
    [SerializeField] private Vector3 endPosUp;

    private bool isRunningCoroutine = false;

    private void Awake() {

        Instance = this; 
    }

    private IEnumerator OpenTransitionCoroutine() {

        isRunningCoroutine = true;

        ShowImage();

        yield return null;

        downImage.anchoredPosition = Vector3.zero;
        upImage.anchoredPosition = Vector3.zero;

        float elapsed = 0f;
        float duration = 1f; 

        yield return null;
        
        while(elapsed <= duration) {

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float easeInT = t * t * t;

            downImage.anchoredPosition = Vector3.Lerp(Vector3.zero, endPosDown, easeInT);
            upImage.anchoredPosition = Vector3.Lerp(Vector3.zero, endPosUp, easeInT);

            yield return null;
        }

        downImage.anchoredPosition = endPosDown;
        upImage.anchoredPosition = endPosUp;

        yield return null;

        HideImage();

        isRunningCoroutine = false;
    }

    private IEnumerator CloseTransitionCoroutine() {

        isRunningCoroutine = true;

        ShowImage();

        yield return null;

        downImage.anchoredPosition = endPosDown;
        upImage.anchoredPosition = endPosUp;

        float elapsed = 0f;
        float duration = 1f;

        yield return null;

        while(elapsed <= duration) {

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float easeOutT = 1f - Mathf.Pow(1f - t, 3f);

            downImage.anchoredPosition = Vector3.Lerp(endPosDown, Vector3.zero, easeOutT);
            upImage.anchoredPosition = Vector3.Lerp(endPosUp, Vector3.zero, easeOutT);

            yield return null;
        }

        downImage.anchoredPosition = Vector3.zero;
        upImage.anchoredPosition = Vector3.zero;

        yield return null;

        isRunningCoroutine = false;
    }

    private void ShowImage() {
        downImage.gameObject.SetActive(true);
        upImage.gameObject.SetActive(true);
    }

    private void HideImage() {
        downImage.gameObject.SetActive(false);
        upImage.gameObject.SetActive(false);
    }

    public void OpenTransition() {
        StartCoroutine(OpenTransitionCoroutine());
    }

    public void CloseTransition() {
        StartCoroutine(CloseTransitionCoroutine());
    }

    public bool IsRunningCoroutine() {
        return this.isRunningCoroutine;
    }
}
