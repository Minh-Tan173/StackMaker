using System.Collections.Generic;
using UnityEngine;

public class BrickPool : MonoBehaviour
{
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private int initBrickCount;

    private Stack<GameObject> brickPool;

    private void Awake() {

        brickPool = new Stack<GameObject>();

        OnInit();
    }

    private void OnInit() {
        // Preload 

        for (int i = 0; i < initBrickCount; i++) {

            GameObject brickObj = SpawnBrick();
            brickObj.transform.SetParent(this.transform);

            HideBrick(brickObj);
            brickPool.Push(brickObj);
        }
    }

    private GameObject SpawnBrick() {
        return Instantiate(brickPrefab);
    }

    private void ShowBrick(GameObject brickObj) {
        brickObj.SetActive(true);
    }
    private void HideBrick(GameObject brickObj) {
        brickObj.SetActive(false);
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent) {

        GameObject brickObj;

        if (brickPool.Count > 0) {
            // If pool is not out of brick

            brickObj = brickPool.Pop();
        }
        else {
            // If pool is out of brick

            brickObj = SpawnBrick();
        }

        // Setup brick object
        brickObj.transform.SetPositionAndRotation(position, rotation);
        brickObj.transform.SetParent(parent);
        ShowBrick(brickObj);

        return brickObj;
    }

    public void Despawn(GameObject brickObj) {

        HideBrick(brickObj);
        brickObj.transform.SetParent(this.transform);
        brickPool.Push(brickObj);
    }
}
