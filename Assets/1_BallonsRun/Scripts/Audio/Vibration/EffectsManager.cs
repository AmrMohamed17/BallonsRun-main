using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{


    public void Vibrate()
    {
        VibrationWithDelay(100, 0.1f);
    }

    public void VibrationWithDelay(long milliseconds, float timer) // #param1 Duration, #param2 Delay
    {
#if UNITY_ANDROID || UNITY_IOS
        if (PlayerPrefs.GetInt("Vibrate", 0) == 0)
            StartCoroutine(VibrateDelay(milliseconds, timer));
#endif
    }

    IEnumerator VibrateDelay(long milliseconds, float timer)
    {
#if UNITY_ANDROID || UNITY_IOS
        yield return new WaitForSeconds(timer);
        Vibration.Vibrate(milliseconds);
#else
        yield break;
#endif
    }

    public void VibrateHandle()
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

}
