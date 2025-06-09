using UnityEngine;
using DG.Tweening;

public class SecretTunnel : MonoBehaviour
{
    public bool tunnel_2;
    public bool tunnel_3;

    public GameObject saw_1;
    public GameObject saw_2;
    public GameObject saw_3;

    public GameObject rocketCannon;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameObject.GetComponent<SpriteRenderer>().DOFade(1, 2);
            gameObject.GetComponent<Collider2D>().enabled = false;

            if(tunnel_2)
            {
                saw_1.SetActive(true);
                saw_2.SetActive(true);
                saw_3.SetActive(true);
            }
            if(tunnel_3)
            {
                rocketCannon.SetActive(true);
            }
        }
    }
}