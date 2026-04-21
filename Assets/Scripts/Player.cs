using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Player : MonoBehaviour
{
    [Header("Child")]
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Transform stackContainer;

    [Header("Data")]
    [SerializeField] private float moveSpeed;

    [Header("Layer")]
    [SerializeField] private LayerMask platformLayer;
    

    private Rigidbody rbPlayer;

    #region Movement Behavior
    private Vector3 moveDir;
    private Vector3 targetPos;
    private bool canMove = false;
    private GameInput.Direct currentDirect;
    private Corner pendingCorner;
    #endregion

    private void Awake() {

        rbPlayer = GetComponent<Rigidbody>();
    }

    private void Start() {

        GameInput.Instance.OnMovedCommand += GameInput_OnMovedCommand;

        OnInit();
    }

    private void OnDestroy() {

        GameInput.Instance.OnMovedCommand -= GameInput_OnMovedCommand;
    }

    private void GameInput_OnMovedCommand(object sender, GameInput.OnMovedCommandEventArgs e) {

        if (canMove) {
            // Is is moving --> dont get new input
            return;
        }

        this.currentDirect = e.moveDirect;
        Vector3 inputVector = GetMoveDir(currentDirect);

        StartNewSegment(inputVector);
    }

    private void Update() {
        
        if (!canMove) {
            return;
        }
            
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float sqrDistance = (targetPos - this.transform.position).sqrMagnitude;

        if (sqrDistance <= 0.1f * 0.1f) {

            this.transform.position = targetPos;
            canMove = false;

            if (pendingCorner != null) {
                // If having Pending Corner

                GameInput.Direct nextDirect = pendingCorner.GetOtherDir(this.currentDirect);

                if (nextDirect != GameInput.Direct.Default) {
                    currentDirect = nextDirect;
                    StartNewSegment(GetMoveDir(this.currentDirect));
                }

                pendingCorner = null;
            }
        }
       
    }

    private void OnInit() {

        // Get Spawn Pos
        Vector2Int startPathNode = ChunkGenerator.Instance.GetLevelSO().chunkList[0].pathNodeList[0].nodePos;
        Vector3 spawnPos = ChunkGenerator.Instance.GetChunkList()[0].gridNodeDict[startPathNode].transform.position;
        Vector3 spawnPosOffset = spawnPos + Vector3.up * 2f;

        // Set Player pos
        this.transform.position = spawnPosOffset;
    }

    private void StartNewSegment(Vector3 moveDir) {

        this.moveDir = moveDir;

        float maxDistance = 100f;
        RaycastHit[] hitArray = Physics.RaycastAll(this.transform.position, moveDir, maxDistance, platformLayer);

        if (hitArray.Length > 0) {

            List<RaycastHit> sortedHit = hitArray.OrderBy(h => (h.transform.position - this.transform.position).sqrMagnitude).ToList();

            for (int i = 0; i < sortedHit.Count; i++) {

                if (sortedHit[i].collider.TryGetComponent<Platform>(out Platform platform)) {

                    // Find nearest Floor Node
                    if (platform.GetNodeID() == GridNode.NodeID.Floor) {

                        if (i == 0) { break; } // Đối diện moveDir là floor

                        targetPos = SnapToGrid(sortedHit[i - 1].transform.position);
                        canMove = true;

                        break;
                    }
                }
            }
        }
    }

    private Vector3 SnapToGrid(Vector3 pos) {
        return new Vector3(Mathf.Round(pos.x), transform.position.y, Mathf.Round(pos.z));
    }

    private Vector3 GetMoveDir(GameInput.Direct inputDir) {

        switch (inputDir) {
            case GameInput.Direct.Forward: return new Vector3(1, 0, 0);
            case GameInput.Direct.Back: return new Vector3(-1, 0, 0);
            case GameInput.Direct.Right: return new Vector3(0, 0, -1f);
            case GameInput.Direct.Left: return new Vector3(0f, 0f, 1f);

            default: return Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other) {


        if (other.TryGetComponent<Platform>(out Platform platform)) {
            
            if (platform.HasCornerOn()) {

                this.pendingCorner = other.GetComponentInChildren<Corner>();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, moveDir * 5f);

        if (targetPos != Vector3.zero) {

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(targetPos, new Vector3(0.8f, 0.1f, 0.8f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPos);
        }
    }
#endif
}