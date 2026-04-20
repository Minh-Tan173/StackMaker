using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Child")]
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Transform stackContainer;

    [Header("Data")]
    [SerializeField] private float moveSpeed;

    private void Start() {

        GameInput.Instance.OnMovedCommand += GameInput_OnMovedCommand;
    }

    private void GameInput_OnMovedCommand(object sender, GameInput.OnMovedCommandEventArgs e) {
        
        
    }
}
