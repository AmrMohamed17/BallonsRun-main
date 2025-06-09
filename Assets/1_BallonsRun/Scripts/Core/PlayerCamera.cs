using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerCamera : MonoBehaviour
{
    // Camera References
    GameObject player;
    public float smoothTime = 0.3f;
    public Vector2 offset = new Vector2(0, 2f);
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public Transform[] spawnPoints;
    Transform spawnPoint;
    PhotonView view;
    private Vector3 velocity = Vector3.zero;
    private void Start()
    {
        spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        player = PhotonNetwork.Instantiate(ShopManager.Instance.Selected_Player, spawnPoint.position, spawnPoint.rotation);

        view = player.GetComponent<PhotonView>();

    }

    void LateUpdate()
    {
        if (view.IsMine)
        {
            // Determine the target position based on player position + offset
            Vector3 targetPosition = new Vector3(player.transform.position.x + offset.x, player.transform.position.y + offset.y, transform.position.z);

            // Clamp the target position to stay within the given bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

            // Smoothly move the camera towards the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

}
