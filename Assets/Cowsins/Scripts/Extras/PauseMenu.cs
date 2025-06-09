using UnityEngine;
using UnityEngine.SceneManagement;

namespace cowsins2D
{
    public class PauseMenu : MonoBehaviour
    {

        public static PauseMenu Instance { get; private set; }

        public static bool isPaused { get; private set; }

        [SerializeField] private PlayerStats stats;

        [SerializeField] private CanvasGroup menu;

        [SerializeField] private float fadeSpeed;

        public PlayerInputs PlayerInput { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;

            isPaused = false;
            menu.gameObject.SetActive(false);
            menu.alpha = 0;
        }

        private void Update()
        {
            PlayerInput = InputManager.playerInputs;
            if (PlayerInput.pausing)
            {
                isPaused = !isPaused;
                if (!isPaused)
                {
                    stats.CheckIfCanGrantControl();
                }
            }

            if (isPaused)
            {
                PlayerStats.LoseControl();
                if (!menu.gameObject.activeSelf)
                {
                    menu.gameObject.SetActive(true);
                    menu.alpha = 0;
                }
                if (menu.alpha < 1) menu.alpha += Time.deltaTime * fadeSpeed;

            }
            else
            {
                menu.alpha -= Time.deltaTime * fadeSpeed;
                if (menu.alpha <= 0) menu.gameObject.SetActive(false);
            }
        }

        public void UnPause()
        {
            isPaused = false;
            stats.CheckIfCanGrantControl();
            Crosshair.Instance.CheckIfCanShow();
            Cursor.visible = false;
        }

        public void QuitGame() => Application.Quit();

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                stats.CheckIfCanGrantControl();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

            }
        }

        public void LoadScene(int id)
        {
            SceneManager.LoadScene(id);
        }

    }
}
