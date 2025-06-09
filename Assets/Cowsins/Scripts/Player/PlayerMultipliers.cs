using UnityEngine;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class PlayerMultipliers : MonoBehaviour
    {
        [SerializeField] private UnityEvent OnBecomeInvincible, OnStopInvincibility;
        [HideInInspector]
        public float speedModifier = 1;

        [HideInInspector]
        public float damageModifier = 1;

        [HideInInspector]
        public float jumpHeightModifier = 1;

        [HideInInspector]
        public float damageReceivedModifier = 1;

        [HideInInspector]
        public float healingReceivedModifier = 1;

        [HideInInspector]
        public bool invincible = false;

        public static PlayerMultipliers Instance { get; private set; }

        private void Awake()
        {
            // There can only be one running instance of this in the game.
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;
        }
        public void StartInvincibility() => invincible = true;

        public void StopInvincibility() => invincible = false;
    }

}