using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Player player;

    private const string TRIGGER_MOVING = "Trigger_Moving";
    private const string TRIGGER_WINNING = "Trigger_Winning";

    private Animator animator;

    private void Awake() {

        animator = GetComponent<Animator>();
    }

    private void Start() {

        player.StartMoving += Player_StartMoving;
    }

    private void OnDestroy() {

        player.StartMoving -= Player_StartMoving;
    }

    private void Player_StartMoving(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_MOVING);
    }
}
