using UnityEngine;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class CustomTrigger : MonoBehaviour
    {
        [SerializeField] private UnityEvent action;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;
            // Perform the custom event if the player triggered this object.
            action?.Invoke();
        }
    }
}
