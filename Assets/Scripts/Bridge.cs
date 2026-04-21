using UnityEngine;

public class Bridge : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform stackVisual;

    private bool isOnStackVisual;

    private void Start() {

        // After spawn
        HideStack();
    }

    private void HideStack() {

        isOnStackVisual = false;
        stackVisual.gameObject.SetActive(false);
    }

    public void ShowStack() {

        isOnStackVisual = true;
        stackVisual.gameObject.SetActive(true);
    }

    public bool IsOnStackVisual() {
        return this.isOnStackVisual;
    }

    public static void SpawnBridge(Transform bridgePrefab, Transform parent, Vector3 spawnPos , Vector3 angleRotation) {

        Transform bridgeTransform = Instantiate(bridgePrefab, parent);
        bridgeTransform.gameObject.name = $"Bridge_{spawnPos.x}_{spawnPos.z}";

        bridgeTransform.localPosition = spawnPos;
        bridgeTransform.localRotation = Quaternion.Euler(angleRotation);
    }
}
