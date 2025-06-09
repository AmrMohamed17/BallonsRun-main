using UnityEngine;
using TMPro;

namespace cowsins2D
{
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField, Tooltip("TMPro Text to display the frame rate:")] TextMeshProUGUI frameRateText;
        [SerializeField, Tooltip("Seconds interval to display the information.")] float updateFrequency = 0.5f;
        [SerializeField, Tooltip("")] bool showCurrentFrameRate = true;
        [SerializeField, Tooltip("")] bool showMinimumFrameRate = true;
        [SerializeField, Tooltip("")] bool showMaximumFrameRate = true;

        private float deltaTime = 0.0f;
        private float currentFPS;
        private float minFPS = Mathf.Infinity;
        private float maxFPS = 0f;

        private Color redColor = Color.red;
        private Color yellowColor = Color.yellow;
        private Color greenColor = Color.green;

        private void Start()
        {
            // Set the initial text values
            UpdateFrameRateText();
        }

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            currentFPS = 1.0f / deltaTime;

            // Update min and max frame rate values
            minFPS = Mathf.Min(minFPS, currentFPS);
            maxFPS = Mathf.Max(maxFPS, currentFPS);

            // Update the frame rate text at the specified update frequency
            if (Time.unscaledTime % updateFrequency <= 0.02f)
            {
                UpdateFrameRateText();
            }
        }

        private void UpdateFrameRateText()
        {
            string text = "";

            if (showCurrentFrameRate)
                text += "Current FPS: " + GetColoredFPSText(currentFPS) + "\n";

            if (showMinimumFrameRate)
                text += "Min FPS: " + GetColoredFPSText(minFPS) + "\n";

            if (showMaximumFrameRate)
                text += "Max FPS: " + GetColoredFPSText(maxFPS);

            frameRateText.text = text;
        }

        private string GetColoredFPSText(float fps)
        {
            Color fpsColor;

            if (fps < 15f)
            {
                fpsColor = redColor;
            }
            else if (fps < 45f)
            {
                fpsColor = yellowColor;
            }
            else
            {
                fpsColor = greenColor;
            }

            return "<color=#" + ColorUtility.ToHtmlStringRGB(fpsColor) + ">" + fps.ToString("F0") + "</color>";
        }
    }

}