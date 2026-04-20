using UnityEngine;

public class Bridge : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform stackVisual;

    private void Start() {

        // After spawn
        HideStack();
    }

    private void ShowStack() {
        stackVisual.gameObject.SetActive(true);
    }

    private void HideStack() {
        stackVisual.gameObject.SetActive(false);
    }

    public static void SpawnBridge(Transform bridgePrefab, Transform parent, Vector3 spawnPos , Vector3 angleRotation) {

        Transform bridgeTransform = Instantiate(bridgePrefab, parent);
        bridgeTransform.gameObject.name = $"Bridge_{spawnPos.x}_{spawnPos.z}";

        bridgeTransform.localPosition = spawnPos;
        bridgeTransform.localRotation = Quaternion.Euler(angleRotation);
    }
}
