using UnityEngine;

public class FinishFlag : MonoBehaviour
{
    public GameObject fireWorks;

    public AudioSource fireworkSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            fireWorks.SetActive(true);
            fireworkSound.Play();
        }
    }
}