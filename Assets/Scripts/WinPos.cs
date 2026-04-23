using System.Collections;
using UnityEngine;

public class WinPos : MonoBehaviour {

    [Header("Point")]
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform victoryPoint;

    [Header("Chest")]
    [SerializeField] private Transform chestClosed;
    [SerializeField] private Transform chestOpened;

    [Header("Partical")]
    [SerializeField] private ParticleSystem[] particleSystemArray;

    private const string PLAYER_TAG = "Player";

    private void Start() {

        OnInit();
    }

    private void OnInit() {

        CloseChest();
        HidePartical();
    }

    public Transform GetTargetPoint() {
        return this.targetPoint;
    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag(PLAYER_TAG)) {

            StartCoroutine(WiningCoroutine(other.GetComponent<Player>()));

        }
    }

    private IEnumerator WiningCoroutine(Player player) {

        LevelManager.Instance.OnWin();

        yield return null;
        
        // WinPos event
        ShowPartical();
        OpenChest();

        // Player event
        player.transform.position = victoryPoint.position;
        player.LookAt(chestOpened);

        yield return new WaitForSeconds(5f);

        HidePartical();
    }

    private void ShowPartical() {
        
        foreach (ParticleSystem particle in particleSystemArray) {

            particle.gameObject.SetActive(true);

            particle.Stop();
            particle.Clear();
            particle.Play();
        }
    }

    private void HidePartical() {

        foreach (ParticleSystem particle in particleSystemArray) {
            particle.gameObject.SetActive(false);
        }
    }

    private void OpenChest() {

        chestClosed.gameObject.SetActive(false);
        chestOpened.gameObject.SetActive(true);
    }

    private void CloseChest() {

        chestClosed.gameObject.SetActive(true);
        chestOpened.gameObject.SetActive(false);
    }

}
