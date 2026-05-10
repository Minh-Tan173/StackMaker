using UnityEngine;

public class Bridge : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform stackVisual;

    [Header("Ref")]
    [SerializeField] private BoxCollider boxCollider;

    private bool isOnStackVisual;

    private void Start() {

        // After spawn
        HideStack();
    }

    public void HideStack() {

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

    public BoxCollider GetBoxCollider() {
        return this.boxCollider;
    }

    public static Bridge SpawnBridge(Bridge bridgePrefab, Transform parent, Vector3 spawnPos , Vector3 angleRotation) {

        Bridge bridge = Instantiate(bridgePrefab, parent);
        bridge.gameObject.name = $"Bridge_{spawnPos.x}_{spawnPos.z}";

        bridge.transform.localPosition = spawnPos;
        bridge.transform.localRotation = Quaternion.Euler(angleRotation);

        return bridge;
    }
}
