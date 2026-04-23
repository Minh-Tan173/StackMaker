using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Player player;

    private const string TRIGGER_MOVING = "Trigger_Moving";
    private const string TRIGGER_WINNING = "Trigger_Winning";
    private const string IDLE_ANIM = "Idle";

    private Animator animator;

    private void Awake() {

        animator = GetComponent<Animator>();
    }

    private void Start() {

        player.StartMoving += Player_StartMoving;
        player.ResetAnimator += Player_ResetAnimator;

        LevelManager.Instance.OnWinState += LevelManager_OnWinState;
    }

    private void OnDestroy() {

        player.StartMoving -= Player_StartMoving;

        LevelManager.Instance.OnWinState -= LevelManager_OnWinState;
    }

    private void LevelManager_OnWinState(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_WINNING);
    }

    private void Player_ResetAnimator(object sender, System.EventArgs e) {

        animator.Rebind();
        animator.Play(IDLE_ANIM);
        animator.Update(0f);
        
    }

    private void Player_StartMoving(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_MOVING);
    }
}
