using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    public float countDuration = 1;
    public TextMeshProUGUI CoinsText;
    float currentValue = 0, targetValue = 0;
    public string Key = "Coins";
    Coroutine _C2T;

    void Start()
    {
        Instance = this;
        CoinsText.text = PlayerPrefs.GetInt(Key, 0).ToString();
        currentValue = float.Parse(CoinsText.text);
        targetValue = currentValue;
    }

    IEnumerator CountTo(float targetValue)
    {
        var rate = Mathf.Abs(targetValue - currentValue) / countDuration;
        while (currentValue != targetValue)
        {
            currentValue = Mathf.MoveTowards(currentValue, targetValue, rate * Time.deltaTime);
            if (NumbersFormater.Instance != null)
            {
                string CoinsNumber = NumbersFormater.Instance.FormattedNumber((int)currentValue);
                CoinsText.text = CoinsNumber.ToString();
            }
            yield return null;
        }
    }

    public void AddValue(float value)
    {
        Debug.Log("AddValue");
        targetValue += value;
        PlayerPrefs.SetInt(Key, (int)targetValue);
        ShopManager.Instance.UpdateUI();
        if (_C2T != null)
            StopCoroutine(_C2T);
        _C2T = StartCoroutine(CountTo(targetValue));
    }

    public void SetTarget(float target)
    {
        targetValue = target;
        if (_C2T != null)
            StopCoroutine(_C2T);
        _C2T = StartCoroutine(CountTo(targetValue));
    }
}
