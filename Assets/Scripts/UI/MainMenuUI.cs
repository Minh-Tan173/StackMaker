using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

    [Header("Button")]
    [SerializeField] private Button startButton;

    private void Awake() {

        startButton.onClick.AddListener(() => {

            LevelManager.Instance.OnPlay();

            Hide();
        });

    }

    private void Start() {


        // When start game 
        Show();
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }
}
