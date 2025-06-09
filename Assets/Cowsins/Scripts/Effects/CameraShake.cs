using UnityEngine;
using UnityEngine.Events;
namespace cowsins2D
{
    public class CameraShake : MonoBehaviour
    {
        #region variables

        public UnityEvent onStartShake, onStartExplosionShake;

        // Current shake amount
        private float trauma;
        public float Trauma
        {
            get
            {
                return trauma;
            }
            set
            {
                trauma = Mathf.Clamp01(value);
            }
        }

        private float power = 16;

        private float movementAmount = 0.8f;

        private float rotationAmount = 17f;

        private float traumaDepthMag = 0.6f;

        private float traumaDecay = 1.3f;

        private float timeCounter = 0;
        #endregion

        static CameraShake _instance;
        public static CameraShake Instance
        {
            get
            {
                return _instance;
            }
        }

        #region methods

        // Set Singleton
        private void Awake() => _instance = this;

        // Utilize a Perlin Noise texture for the camera shake

        private float GetFloat(float seed) { return (Mathf.PerlinNoise(seed, timeCounter) - 0.5f) * 2f; }

        private Vector3 GetVec3() { return new Vector3(GetFloat(1), GetFloat(10), GetFloat(100) * traumaDepthMag); }

        private void Update()
        {
            // Apply camera shake only if the current strength is big enough
            if (Trauma > 0)
            {
                timeCounter += Time.deltaTime * Mathf.Pow(Trauma, 0.3f) * power;

                Vector3 newPos = GetVec3() * movementAmount * Trauma;
                transform.localPosition = newPos;

                transform.localRotation = Quaternion.Euler(newPos * rotationAmount);

                Trauma -= Time.deltaTime * traumaDecay * (Trauma + 0.3f);
            }
            else
            {
                //lerp back towards default position and rotation once shake is done
                Vector3 newPos = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime);
                transform.localPosition = newPos;
                transform.localRotation = Quaternion.Euler(newPos * rotationAmount);
            }
        }

        public void Shake(float amount, float _power, float _movementAmount, float _rotationAmount)
        {
            Trauma = amount;
            power = _power;
            movementAmount = _movementAmount;
            rotationAmount = _rotationAmount;

            onStartShake?.Invoke();
        }

        public void ExplosionShake(float distance)
        {
            Trauma += 10f / distance;
            power = 30;
            movementAmount = 1f;
            rotationAmount = 30f;

            onStartExplosionShake?.Invoke();
        }

        #endregion
    }

}