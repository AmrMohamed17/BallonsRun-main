using UnityEngine;

namespace cowsins2D
{
    public class CheckPointManager : MonoBehaviour
    {
        // Stores the player's last checkpoint position.
        public static Vector3? lastCheckpoint { get; private set; } = null;
        public static void Checkpoint(Transform obj)
        {
            // Store the player's current position as the last checkpoint.
            lastCheckpoint = obj.position;

            // Destroy the checkpoint object.
            Destroy(obj.gameObject);
        }
    }

}