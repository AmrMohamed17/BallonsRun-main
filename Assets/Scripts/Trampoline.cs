using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            anim.SetTrigger("up");
            //collision.gameObject.GetComponent<PlayerMovement>().jumpForce = 25f;
        }
    }
}