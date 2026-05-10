using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    [Header("Button")]
    [SerializeField] private Button startButton;

    private void Awake() {

        Instance = this;
    }

    private void Start() {

        startButton.onClick.AddListener(() => {

            LevelManager.Instance.OnPlay();

            Hide();
        });

        // When start game 
        Show();
    }

    public void Show() {
        this.gameObject.SetActive(true);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }
}
