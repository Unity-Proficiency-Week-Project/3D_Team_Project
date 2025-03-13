using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance;
    public event Action OnWeatherChanged;

    public enum WeatherType { Sunny, Cloudy, Rainy, Snowy}
    public WeatherType currentWeather = WeatherType.Sunny;

    public float globalTemp = 18f;
    public float weatherChangeInterval = 60f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WeatherLoop());
    }

    private IEnumerator WeatherLoop()
    {
        while (true)
        {
            ChangeWeather();
            yield return new WaitForSeconds(weatherChangeInterval);
        }
    }

    private void ChangeWeather()
    {
        float currentLocationTemp = 18f;
        if (PlayerManager.Instance?.Player?.temp != null)
        {
            currentLocationTemp = PlayerManager.Instance.Player.temp.GetNearestTemp();
        }
        WeatherType newWeather = GetWeatherBasedOnTemp(currentLocationTemp);

        currentWeather = newWeather;
        globalTemp = GetGlobalTempFromWeather(newWeather);

        Debug.Log($"날씨 변경: {currentWeather}, 글로벌 온도: {globalTemp}°C");
        OnWeatherChanged?.Invoke();
    }

    private WeatherType GetWeatherBasedOnTemp(float temp)
    {
        float sunnyWeight = 1f;
        float cloudyWeight = 1f;
        float rainyWeight = 1f;
        float snowyWeight = 1f;

        // 온도별 확률 조정
        if (temp >= 35f) // 매우 더운 지역 (사막)
        {
            sunnyWeight = 5f; // 맑음 확률 증가
            rainyWeight = 0.5f; // 비 올 확률 낮춤
            snowyWeight = 0.1f; // 눈 확률 매우 낮게
        }
        else if (temp >= 25f) // 따뜻한 지역
        {
            sunnyWeight = 4f;
            cloudyWeight = 2f;
            rainyWeight = 1.5f;
            snowyWeight = 0.2f;
        }
        else if (temp >= 10f) //  온화한 지역
        {
            sunnyWeight = 2f;
            cloudyWeight = 3f;
            rainyWeight = 2f;
            snowyWeight = 0.5f;
        }
        else if (temp >= 0f) //  추운 지역
        {
            sunnyWeight = 1f;
            cloudyWeight = 2f;
            rainyWeight = 2f;
            snowyWeight = 3f; // 눈 확률 증가
        }
        else // ❄️ 매우 추운 지역 (-5°C 이하)
        {
            sunnyWeight = 0.5f;
            cloudyWeight = 1f;
            rainyWeight = 0.2f;
            snowyWeight = 5f; // 눈 확률 최대로 증가
        }

        // 가중치 기반 확률 계산
        float totalWeight = sunnyWeight + cloudyWeight + rainyWeight + snowyWeight;
        float randomValue = UnityEngine.Random.Range(0, totalWeight);

        if (randomValue < sunnyWeight)
            return WeatherType.Sunny;
        else if (randomValue < sunnyWeight + cloudyWeight)
            return WeatherType.Cloudy;
        else if (randomValue < sunnyWeight + cloudyWeight + rainyWeight)
            return WeatherType.Rainy;
        else
            return WeatherType.Snowy;
    }

    private float GetGlobalTempFromWeather(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Sunny: return 0.15f;
            case WeatherType.Cloudy: return -0.05f;
            case WeatherType.Rainy: return -0.1f;
            case WeatherType.Snowy: return -0.3f;
            default: return 0f;
        }
    }

}
