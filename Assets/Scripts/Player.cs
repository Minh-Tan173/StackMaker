using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Child")]
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Transform stackContainer;

    [Header("Data")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float checkDistance;
    

    private Rigidbody rbPlayer;

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

        Debug.Log($"Movedir: {GetMoveDir(e.moveDirect)}");
        
    }

    private void Update() {
        
        //Physics.Raycast(this.transform.position, )
        
    }

    private void OnInit() {

        // Get Spawn Pos
        Vector2Int startPathNode = ChunkGenerator.Instance.GetLevelSO().chunkList[0].pathNodeList[0].nodePos;
        Vector3 spawnPos = ChunkGenerator.Instance.GetChunkList()[0].gridNodeDict[startPathNode].transform.position;
        Vector3 spawnPosOffset = spawnPos + Vector3.up * 2f;

        // Set Player pos
        this.transform.position = spawnPosOffset;
    }

    private Vector2Int GetMoveDir(GameInput.Direct inputDir) {

        switch (inputDir) {
            case GameInput.Direct.Forward: return new Vector2Int(1, 0);
            case GameInput.Direct.Back: return new Vector2Int(-1, 0);
            case GameInput.Direct.Right: return new Vector2Int(0, -1);
            case GameInput.Direct.Left: return new Vector2Int(0, 1);
                
            default: return Vector2Int.zero;
        }
    }
}
