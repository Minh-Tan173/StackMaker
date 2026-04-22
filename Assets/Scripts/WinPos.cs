using UnityEngine;

public class WinPos : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;

    [Header("Partical")]
    [SerializeField] private ParticleSystem[] particleSystemArray;

    private const string PLAYER_TAG = "Player";

    public Transform GetTargetPoint() {
        return this.targetPoint;
    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag(PLAYER_TAG)) {

        }
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
