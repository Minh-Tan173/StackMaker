using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour {

    // SFX event
    public static event EventHandler OnAddBrickSFX;
    public static event EventHandler OnRemoveBrickSFX;
    
    public event EventHandler StartMoving;
    public event EventHandler ResetAnimator;

    [Header("Child")]
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Transform stackContainer;

    [Header("Data")]
    [SerializeField] private float moveSpeed;

    [Header("Layer")]
    [SerializeField] private LayerMask platformLayer;

    [Header("Brick")]
    [SerializeField] private Transform brickPrefab;
    [SerializeField] private float brickHeight;
    [SerializeField] private BrickPool brickPool;

    private Stack<Transform> brickCollection;

    private Vector3 playerVisualLocalPos;

    #region Movement Behavior
    private Vector3 moveDir;
    private Vector3 startPos;
    private Vector3 targetPos;
    private bool canMove = false;
    private bool canInteract = false;
    private bool isTurnBack = false;
    private InputManager.Direct currentDirect;
    private Corner pendingCorner;
    private List<Bridge> bridgeList;
    #endregion

    private void Awake() {

        brickCollection = new Stack<Transform>();

        bridgeList = new List<Bridge>();

        playerVisualLocalPos = playerVisual.localPosition;
    }

    private void Start() {

        LevelManager.Instance.OnWinState += LevelManager_OnWinState;
        LevelManager.Instance.InitObjectData += LevelManager_InitObjectData;
        LevelManager.Instance.ClearObjectData += LevelManager_ClearObjectData;

        InputManager.Instance.OnMovedCommand += InputManager_OnMovedCommand;
    }

    private void OnDestroy() {

        LevelManager.Instance.OnWinState -= LevelManager_OnWinState;
        LevelManager.Instance.InitObjectData -= LevelManager_InitObjectData;
        LevelManager.Instance.ClearObjectData -= LevelManager_ClearObjectData;

        InputManager.Instance.OnMovedCommand -= InputManager_OnMovedCommand;

    }

    private void LevelManager_ClearObjectData(object sender, EventArgs e) {

        OnDespawn();
    }

    private void LevelManager_InitObjectData(object sender, EventArgs e) {

        ClearBrick();

        OnInit();
    }

    private void LevelManager_OnWinState(object sender, EventArgs e) {

        canMove = false;

        ClearBrick();
    }

    private void InputManager_OnMovedCommand(object sender, InputManager.OnMovedCommandEventArgs e) {

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

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        
        float sqrDistance = (targetPos - this.transform.position).sqrMagnitude;

        if (sqrDistance <= 0.1f * 0.1f) {

            this.transform.position = targetPos;
            canMove = false;

            if (pendingCorner != null) {
                // If having Pending Corner

                InputManager.Direct nextDirect = pendingCorner.GetOtherDir(this.currentDirect);

                if (nextDirect != InputManager.Direct.Default) {
                    currentDirect = nextDirect;
                    StartNewSegment(GetMoveDir(this.currentDirect));
                }

                pendingCorner = null;
            }

            if (isTurnBack) {
                // Was turn back from Bridge

                isTurnBack = false;
            }
        }
       
    }

    private void OnInit() {

        // Setup Spawn position
        Vector2Int startPathNode = ChunkGenerator.Instance.GetLevelSO().chunkList[0].pathNodeList[0].nodePos;
        Vector3 spawnPos = ChunkGenerator.Instance.GetChunkList()[0].gridNodeDict[startPathNode].transform.position;
        Vector3 spawnPosOffset = spawnPos + Vector3.up * 0.5f;

        this.transform.position = spawnPosOffset;

        // Setup playerVisual rotation
        LookAt(null);

        // Setup animator
        ResetAnimator?.Invoke(this, EventArgs.Empty);

        // Reset Interact behavior
        canInteract = true;
    }

    private void OnDespawn() {

        ClearBrick();

        canMove = false;
        canInteract = false;
        isTurnBack = false;

        brickCollection.Clear();
        bridgeList.Clear();
    }

    private void StartNewSegment(Vector3 moveDir) {

        bridgeList.Clear(); // Clear bridgeList beform new Movement Progress

        this.moveDir = moveDir;

        float maxDistance = 100f;
        RaycastHit[] hitArray = Physics.RaycastAll(this.transform.position, moveDir, maxDistance, platformLayer);

        if (hitArray.Length > 0) {

            List<RaycastHit> sortedHit = hitArray.OrderBy(h => (h.transform.position - this.transform.position).sqrMagnitude).ToList();
            
            for (int i = 0; i < sortedHit.Count; i++) {

                // Handle if find WinPos 
                if (sortedHit[i].collider.TryGetComponent<WinPos>(out WinPos winPos)) {

                    startPos = this.transform.position;

                    targetPos = winPos.GetTargetPoint().position;
                    canMove = true;

                    break;
                }

                // Handle if find Floor
                if (sortedHit[i].collider.TryGetComponent<Platform>(out Platform platform)) {

                    if (platform.GetNodeID() == GridNode.NodeID.Wall) {

                        if (i == 0) { break; } // Đối diện moveDir là floor

                        startPos = this.transform.position;

                        targetPos = SnapToGrid(sortedHit[i - 1].transform.position);
                        canMove = true;

                        StartMoving?.Invoke(this, EventArgs.Empty);

                        break;
                    }
                }

                if (sortedHit[i].collider.TryGetComponent<Bridge>(out Bridge bridge)) {

                    bridgeList.Add(bridge);
                }
            }
        }
    }

    private Vector3 SnapToGrid(Vector3 pos) {
        return new Vector3(Mathf.Round(pos.x), transform.position.y, Mathf.Round(pos.z));
    }

    private Vector3 GetMoveDir(InputManager.Direct inputDir) {

        return inputDir switch {
            InputManager.Direct.Forward => new Vector3(1f, 0f, 0f),
            InputManager.Direct.Back => new Vector3(-1f, 0f, 0f),
            InputManager.Direct.Right => new Vector3(0f, 0f, -1f),
            InputManager.Direct.Left => new Vector3(0f, 0f, 1f),

            _ => Vector3.zero
        };
    }

    private void AddBrick() {

        // SFX
        OnAddBrickSFX?.Invoke(this, EventArgs.Empty);

        float newBrickPosY = brickCollection.Count * brickHeight;

        // Update new Brick
        GameObject brickObj = brickPool.Spawn(Vector3.zero, Quaternion.identity, stackContainer);
        brickObj.transform.localPosition = new Vector3(0f, newBrickPosY, 0f);

        brickCollection.Push(brickObj.transform);

        // Update Player Height
        playerVisual.localPosition = playerVisualLocalPos + new Vector3(0f, brickCollection.Count * brickHeight, 0f);
    }


    private void RemoveBrick() {

        OnRemoveBrickSFX?.Invoke(this, EventArgs.Empty);

        // Remove First Brick and return it to the Pool
        Transform topBrick = brickCollection.Pop();
        brickPool.Despawn(topBrick.gameObject);

        // Lower Player Height
        playerVisual.localPosition = playerVisualLocalPos + new Vector3(0f, brickCollection.Count * brickHeight, 0f);
    }

    private void ClearBrick() {
        
        while (brickCollection.Count > 0) {

            Transform topBrick = brickCollection.Pop();
            brickPool.Despawn(topBrick.gameObject);
        }
            
        playerVisual.localPosition = playerVisualLocalPos;
    }

    private void OnTriggerEnter(Collider other) {

        if (!canInteract) { return; }

        if (other.CompareTag(GameTag.PLATFORM_TAG)) {

            ChunkGenerator.Instance.GetPlatformDict().TryGetValue(other, out Platform platform);
            
            if (platform == null) {
                Debug.LogError("This platform dont attached by Platform script");
                return;
            }

            // Handle interaction with Corner
            if (platform.TryGetCorner(out Corner corner)) {

                this.pendingCorner = corner;
            }

            // Handle interaction with Path platform
            if (platform.IsStackVisualOn()) {

                platform.HideStack();

                AddBrick();

            }
        }
        else if (other.CompareTag(GameTag.BRIDGE_TAG)) {
            // Interaction with Bridge

            ChunkGenerator.Instance.GetBridgeDict().TryGetValue(other, out Bridge bridge);

            if (bridge == null) {
                Debug.LogError("This bridge dont attached by Platform script");
                return;
            }

            if (!isTurnBack) {

                if (!bridge.IsOnStackVisual()) {

                    bridge.ShowStack();
                    RemoveBrick();
                }

                float xSizeBridge = bridge.GetBoxCollider().size.x;
                float maxDistance = xSizeBridge + 0.3f;

                int currentBridgeIndex = bridgeList.IndexOf(bridge);
                
                if (currentBridgeIndex < bridgeList.Count - 1) {
                    // If current index is not last index of bridgeList --> Đang đi trên Bridge

                    if (brickCollection.Count == 0) {
                        // Đang đi trên cầu mà hết Brick --> Quay về

                        isTurnBack = true;
                        moveDir = -moveDir;

                        targetPos = startPos;

                        // Hoàn trả lại brick vừa remove ở trên Bridge này
                        bridge.HideStack();
                        AddBrick();

                        // Player hasn't fully left the previous bridge due to collider overlap
                        if (currentBridgeIndex > 0) {
                            
                            Bridge bridgeBefore = bridgeList[currentBridgeIndex - 1];

                            if (bridgeBefore.IsOnStackVisual()) {

                                bridgeBefore.HideStack();
                                AddBrick();
                            }
                        }

                        canMove = true;
                    }
                }
            }
            else {
                // Đang quay ngược về

                if (bridge.IsOnStackVisual()) {

                    bridge.HideStack();
                    AddBrick();
                }
            }
            
        }
    }

    public void LookAt(Transform target = null) {

        if (target != null) {

            Vector3 targetDir = (target.position - this.transform.position).normalized;
            targetDir.y = 0f;

            playerVisual.rotation = Quaternion.LookRotation(targetDir);
        }
        else {

            Vector3 lookAtScreenEuler = new Vector3(0f, -90f, 0f);

            playerVisual.rotation = Quaternion.Euler(lookAtScreenEuler);
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