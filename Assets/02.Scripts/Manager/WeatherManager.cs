using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;
    public event Action OnWeatherChanged;

    public enum WeatherType { Sunny, Cloudy, Rainy, Snowy}
    public WeatherType currentWeather = WeatherType.Sunny;

    public float globalTemp = 18f;
    public float weatherChangeInterval = 60f;

    [Header("ParticleSys")]
    public ParticleSystem snow;
    public ParticleSystem rain;

    public Transform player;

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

        UpdateParticles();
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

    private void UpdateParticles()
    {
        float temp = globalTemp;

        Vector3 newPos = player.position;
        newPos.y += 50;

        if (currentWeather == WeatherType.Rainy)
        {
            snow.gameObject.SetActive(false);
            rain.gameObject.SetActive(true);
            rain.transform.position = newPos;

            AdjustRainParticle(temp);
        }
        else if (currentWeather == WeatherType.Snowy)
        {
            rain.gameObject.SetActive(false);
            snow.gameObject.SetActive(true);
            snow.transform.position = newPos;

            AdjustSnowParticle(temp);
        }
        else
        {
            rain.gameObject.SetActive(false);
            snow.gameObject.SetActive(false);
        }
    }

    private void AdjustRainParticle(float temp)
    {
        var emission = rain.emission;
        var main = rain.main;

        // 온도가 높을수록 비 많이 올 확률 증가 (기본 50% 확률, 최대 90%까지 증가)
        float baseChance = Mathf.Lerp(0.5f, 0.9f, (temp - 20) / 20);
        bool heavyRain = UnityEngine.Random.value < baseChance; // 확률적 강한 비 발생 여부

        // 비 입자 개수 설정 (확률적으로 많거나 적게)
        float minRate = 100;
        float maxRate = 1000;
        emission.rateOverTime = heavyRain ? UnityEngine.Random.Range(maxRate * 0.8f, maxRate) : UnityEngine.Random.Range(minRate, maxRate * 0.3f);

        // 비 입자 크기 설정 (확률적으로 크게 or 작게)
        float minSize = 0.1f;
        float maxSize = 0.2f;
        main.startSize = heavyRain ? UnityEngine.Random.Range(maxSize * 0.7f, maxSize) : UnityEngine.Random.Range(minSize, maxSize * 0.5f);

        Debug.Log($"비 강도: {(heavyRain ? "많음" : "적음")}, 입자 개수: {emission.rateOverTime.constant}, 입자 크기: {main.startSize.constant}");
    }

    private void AdjustSnowParticle(float temp)
    {
        var emission = snow.emission;
        var main = snow.main;

        // 온도가 낮을수록 눈 많이 올 확률 증가 (기본 50% 확률, 최대 90%까지 증가)
        float baseChance = Mathf.Lerp(0.5f, 0.9f, -temp / 10);
        bool heavySnow = UnityEngine.Random.value < baseChance; // 확률적 강한 눈 발생 여부

        // 눈 입자 개수 설정 (확률적으로 많거나 적게)
        float minRate = 100;
        float maxRate = 1000;
        emission.rateOverTime = heavySnow ? UnityEngine.Random.Range(maxRate * 0.8f, maxRate) : UnityEngine.Random.Range(minRate, maxRate * 0.3f);

        // 눈 입자 크기 설정 (확률적으로 크게 or 작게)
        float minSize = 0.1f;
        float maxSize = 1.0f;
        main.startSize = heavySnow ? UnityEngine.Random.Range(maxSize * 0.7f, maxSize) : UnityEngine.Random.Range(minSize, maxSize * 0.5f);

        Debug.Log($"눈 강도: {(heavySnow ? "많음" : "적음")}, 입자 개수: {emission.rateOverTime.constant}, 입자 크기: {main.startSize.constant}");
    }
}