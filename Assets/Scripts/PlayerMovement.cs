using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    // Move References
    public float runSpeed = 6f;
    public float boostSpeed = 10f;
    public float boostTime = 1.5f;
    public float jumpForce = 10f;
    public int maxJumps = 2;

    public Transform SpawnPoint;
    private GameObject BoostEffect;
    private GameObject BoostObject;

    private Rigidbody2D rb;
    private Animator animator;

    private float moveSpeed;
    private bool canMove = false;
    private bool isGrounded = false;
    private bool DoubleJump = false;
    private int currentJumps = 0;

    private SpriteRenderer tunnel;
    private SpriteRenderer foreGround;

    public AudioSource jumpSound;
    public AudioSource boostSound;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        BoostEffect = Resources.Load("BoostEffect") as GameObject;
        DoubleJump = gameObject.name.Contains("BlueRabbit");

        moveSpeed = runSpeed;
        Invoke("StartMoving", 0.1f);

        tunnel = GameObject.FindGameObjectWithTag("SecretTunnel").GetComponent<SpriteRenderer>();
        //foreGround = GameObject.FindGameObjectWithTag("ForeGround").GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            animator.SetBool("run", true);
        }
        else
        {
            animator.SetBool("run", false);
        }

        if (!isGrounded)
        {
            animator.SetBool("jump", true);
        }
        else
        {
            animator.SetBool("jump", false);
        }

        // Handle screen touch for jumping
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
        {
            Jump();
        }
    }

    void StartMoving()
    {
        canMove = true;
    }

    public void Jump()
    {
        // Check if the player is grounded for the first jump
        if (isGrounded)
        {
            // Player Jump
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            currentJumps = 1;  // Set jumps to 1 on first jump
            animator.SetTrigger("jump");
            jumpSound.Play();  // Play jump sound only when actually jumping
        }
        // Check if the player has used less than the max allowed jumps for a double jump
        else if (currentJumps < maxJumps)
        {
            // Player Double Jump
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            currentJumps++; // Increment jump count
            animator.SetTrigger("jump");
            jumpSound.Play();  // Play jump sound only when actually jumping
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            currentJumps = 0;  // Reset jump count on ground
            animator.SetBool("run", canMove);
        }
        if (collision.gameObject.CompareTag("Spike"))
        {
            canMove = false;
            animator.SetTrigger("damage");
            Invoke("StartMoving", 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boost"))
        {
            Debug.Log("Boost");
            collision.enabled = false;
            moveSpeed = boostSpeed;
            BoostObject = Instantiate(BoostEffect, SpawnPoint);
            StartCoroutine(EndBoost());
            boostSound.Play();
        }
        if (collision.gameObject.CompareTag("Tunnel"))
        {
            Debug.Log("Gate_Tunnel");
            collision.GetComponent<Collider2D>().enabled = false;
            tunnel.DOFade(1, 2);
            foreGround.DOFade(0, 2);
        }
        if (collision.gameObject.CompareTag("Finish"))
        {
            canMove = false;
            moveSpeed = 0;
            animator.SetTrigger("win");
            Destroy(GetComponent<Rigidbody2D>());
            GetComponent<Collider2D>().enabled = false;
        }
        if (collision.gameObject.CompareTag("trampolineFinish"))
        {
            jumpForce = 10;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("trampoline"))
        {
            jumpForce = 30f;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("trampoline"))
        {
            jumpForce = 10;
        }
    }

    private IEnumerator EndBoost()
    {
        yield return new WaitForSeconds(boostTime);
        Debug.Log("EndBoost");
        Destroy(BoostObject);
        moveSpeed = runSpeed;
    }
}
