using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumbersFormater : MonoBehaviour
{
    // NumbersFormater Singleton
    public static NumbersFormater Instance;
    private void Awake()
    {
        Instance = this;
    }

    public string FormattedTime(double time)
    {
        // Calculate hours, minutes, and seconds
        int hours = (int)(time / 3600);
        int minutes = (int)((time % 3600) / 60);
        int seconds = (int)(time % 60);

        // Format the string
        string formattedTime = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

        return formattedTime;
    }

    public string FormattedNumber(int number)
    {
        string ResourceAmount = number >= 1000 ? (number / 1000f).ToString("0.#") + "k" : number.ToString();
        return ResourceAmount;
    }
}
