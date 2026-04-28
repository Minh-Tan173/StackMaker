using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour {

    public event EventHandler StartMoving;
    public event EventHandler ResetAnimator;

    [Header("Child")]
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Transform stackContainer;

    [Header("Data")]
    [SerializeField] private float moveSpeed;

    [Header("Layer")]
    [SerializeField] private LayerMask platformLayer;

    [Header("Stack Data")]
    [SerializeField] private Transform stackPrefab;
    [SerializeField] private float stackHeight;
    
    private Rigidbody rbPlayer;
    private Stack<Transform> brickCollection;

    private Vector3 playerVisualLocalPos;

    #region Movement Behavior
    private Vector3 moveDir;
    private Vector3 targetPos;
    private bool canMove = false;
    private InputManager.Direct currentDirect;
    private Corner pendingCorner;
    #endregion

    private void Awake() {

        rbPlayer = GetComponent<Rigidbody>();
        brickCollection = new Stack<Transform>();

        playerVisualLocalPos = playerVisual.localPosition;
    }

    private void Start() {

        LevelManager.Instance.OnWinState += LevelManager_OnWinState;
        LevelManager.Instance.InitObjectData += LevelManager_InitObjectData;

        InputManager.Instance.OnMovedCommand += InputManager_OnMovedCommand;
    }

    private void OnDestroy() {

        LevelManager.Instance.OnWinState -= LevelManager_OnWinState;
        LevelManager.Instance.InitObjectData -= LevelManager_InitObjectData;

        InputManager.Instance.OnMovedCommand -= InputManager_OnMovedCommand;

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

        if (Vector3.Distance(this.transform.position, targetPos) <= 0.1f) {

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
    }

    private void StartNewSegment(Vector3 moveDir) {

        this.moveDir = moveDir;

        float maxDistance = 100f;
        RaycastHit[] hitArray = Physics.RaycastAll(this.transform.position, moveDir, maxDistance, platformLayer);

        if (hitArray.Length > 0) {

            List<RaycastHit> sortedHit = hitArray.OrderBy(h => (h.transform.position - this.transform.position).sqrMagnitude).ToList();
            
            for (int i = 0; i < sortedHit.Count; i++) {

                if (sortedHit[i].collider.TryGetComponent<WinPos>(out WinPos winPos)) {

                    targetPos = winPos.GetTargetPoint().position;
                    canMove = true;

                    break;
                }

                if (sortedHit[i].collider.TryGetComponent<Platform>(out Platform platform)) {

                    // Find nearest Floor Node
                    if (platform.GetNodeID() == GridNode.NodeID.Wall) {

                        if (i == 0) { break; } // Đối diện moveDir là floor

                        targetPos = SnapToGrid(sortedHit[i - 1].transform.position);
                        canMove = true;

                        StartMoving?.Invoke(this, EventArgs.Empty);

                        break;
                    }
                }
            }
        }
    }

    private Vector3 SnapToGrid(Vector3 pos) {
        return new Vector3(Mathf.Round(pos.x), transform.position.y, Mathf.Round(pos.z));
    }

    private Vector3 GetMoveDir(InputManager.Direct inputDir) {

        switch (inputDir) {
            case InputManager.Direct.Forward: return new Vector3(1, 0, 0);
            case InputManager.Direct.Back: return new Vector3(-1, 0, 0);
            case InputManager.Direct.Right: return new Vector3(0, 0, -1f);
            case InputManager.Direct.Left: return new Vector3(0f, 0f, 1f);

            default: return Vector3.zero;
        }
    }

    private void AddBrick() {

        if (brickCollection.Count > 0) {
            // If having stack in stackCollection ---> Up their height and player height before add new stack

            // Up height stack
            foreach (Transform stack in stackContainer) {

                stack.localPosition += Vector3.up * stackHeight;
            }

            // Up height Player
            playerVisual.localPosition += Vector3.up * stackHeight;
        }

        // Spawn new stack
        Transform stackTransform = Instantiate(stackPrefab, stackContainer);
        stackTransform.localPosition = Vector3.zero;

        brickCollection.Push(stackTransform);
    }

    private void RemoveBrick() {

        // Remove first stack
        Transform bottomStack = brickCollection.Pop();
        Destroy(bottomStack.gameObject);

        //
        foreach (Transform brick in brickCollection) {

            brick.localPosition -= Vector3.up * stackHeight;
        }

        //
        playerVisual.localPosition -= Vector3.up * stackHeight;
        
    }

    private void ClearBrick() {
        
        while (brickCollection.Count > 0) {

            Transform bottomBrick = brickCollection.Pop();
            Destroy(bottomBrick.gameObject);
        }

        playerVisual.localPosition = playerVisualLocalPos;
    }

    private void OnTriggerEnter(Collider other) {


        if (other.CompareTag(GameTag.PLATFORM_TAG)) {

            Platform platform = other.GetComponent<Platform>();
            
            if (platform == null) {
                Debug.LogError("This platform dont attached by Platform script");
                return;
            }

            // Handle interaction with Corner
            if (platform.HasCornerOn()) {

                this.pendingCorner = other.GetComponentInChildren<Corner>();
            }

            // Handle interaction with Path plaform
            if (platform.IsStackVisualOn()) {

                platform.HideStack();

                AddBrick();

            }
        }
        else if (other.CompareTag(GameTag.BRIDGE_TAG)) {
            // Interaction with Bridge

            Bridge bridge = other.GetComponent<Bridge>();

            if (bridge == null) {
                Debug.LogError("This bridge dont attached by Platform script");
                return;
            }

            if (!bridge.IsOnStackVisual()) {

                bridge.ShowStack();
                RemoveBrick();
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