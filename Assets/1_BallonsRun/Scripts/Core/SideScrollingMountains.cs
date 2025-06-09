using UnityEngine;

public class SideScrollingMountains : MonoBehaviour
{
    public float scrollSpeed = 2f;
    public float moveDistance = 10f;
    private Vector3 startPosition;
    private bool movingRight = false;

    void Start()
    {
        // Store the initial position
        startPosition = transform.position;
    }

    void Update()
    {
        if (movingRight)
        {
            transform.Translate(Vector2.right * scrollSpeed * Time.deltaTime);

            if (transform.position.x >= startPosition.x + moveDistance)
            {
                movingRight = false;
            }
        }
        else
        {
            transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

            if (transform.position.x <= startPosition.x - moveDistance)
            {
                movingRight = true;
            }
        }
    }
}
