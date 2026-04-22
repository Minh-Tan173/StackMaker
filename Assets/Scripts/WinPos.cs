using System.Collections;
using UnityEngine;

public class WinPos : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;

    [Header("Partical")]
    [SerializeField] private ParticleSystem[] particleSystemArray;

    private const string PLAYER_TAG = "Player";

    private void Start() {

        HidePartical();
    }

    public Transform GetTargetPoint() {
        return this.targetPoint;
    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag(PLAYER_TAG)) {

            StartCoroutine(WiningCoroutine());

        }
    }

    private IEnumerator WiningCoroutine() {

        ShowPartical();

        LevelManager.Instance.OnWin();

        while (particleSystemArray[0].IsAlive()) {

            yield return null;
        }

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

}
