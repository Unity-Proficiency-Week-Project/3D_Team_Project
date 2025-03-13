using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

public class PlayerTemperature : MonoBehaviour
{
    private List<TemperatureZone> currentZones = new List<TemperatureZone>(); // 현재 들어가 있는 지역 리스트

    private float baseTemp = 18f;
    public float currentTemp;
    public float nearestTemp;
    public float coldThreshold = 5f;
    public float hotThreshold = 30f;

    private Coroutine effectCoroutine;

    private PlayerController playerController;
    private PlayerCondition playerCondition;

    public float CurrentTemp => currentTemp;

    private void Start()
    {
        playerController = PlayerManager.Instance.Player.controller;
        playerCondition = PlayerManager.Instance.Player.condition;

        UpdateTemp();

        WeatherSystem.Instance.OnWeatherChanged += UpdateTemp;
    }

    public void EnterTempZone(TemperatureZone zone)
    {
        if (!currentZones.Contains(zone))
        {
            currentZones.Add(zone);
            UpdateTemp(); // 공간에 들어갈 때 온도 갱신
        }
    }

    public void ExitTempZone(TemperatureZone zone)
    {
        if (currentZones.Contains(zone))
        {
            currentZones.Remove(zone);
            UpdateTemp(); // 공간에서 나갈 때 온도 갱신
        }
    }


    public void UpdateTemp()
    {
        nearestTemp = baseTemp;
        if(currentZones.Count > 0)
        {
            nearestTemp = currentZones.Average(zone => zone.temp);
        }

        float extraHeat = CheckNearbyHeatSources();
        // float coldResistance = playerEquipment?.coldResistance ?? 0f;
        float weatherEffect = WeatherSystem.Instance.globalTemp;
        currentTemp = Mathf.Lerp(nearestTemp, nearestTemp * (1 + weatherEffect), 0.5f) + extraHeat; // + coldResistance

        Debug.Log($"현재 온도 갱신: {currentTemp}°C (지역: {nearestTemp}°C, 날씨 영향: {weatherEffect})");
    }
    private IEnumerator AffectLoop()
    {
        while (true)
        {
            ApplyTempEffects();
            yield return new WaitForSeconds(1f); // 1초마다 지속적으로 효과 적용
        }
    }

    private void ApplyTempEffects()
    {
        if (currentTemp >= hotThreshold)
        {
            float diff = currentTemp - hotThreshold;
            float thirstPenalty = Mathf.Pow(diff / 10f, 2); // 온도 차이가 클수록 페널티 증가
            playerCondition.SubtrackThirst(thirstPenalty);
        }

        if(currentTemp <= coldThreshold)
        {
            float diff = coldThreshold - currentTemp;
            float slowFactor = Mathf.Lerp(0.8f, 0.3f, diff / 20f); // 온도 차이에 따라 이동 속도 감소
            float coldDamage = Mathf.Pow(diff / 10f, 2); // 온도 차이가 클수록 피해 증가

            playerController.moveSpeed = playerController.defaultSpeed * slowFactor;
            playerCondition.ColdDamage(coldDamage);
        }
        else
        {
            playerController.moveSpeed = playerController.defaultSpeed;
        }
    }
    //근처 불이 있으면 확인할 코드(현재 구현 X)
    private float CheckNearbyHeatSources()
    {
        return 0;
    }

    public float GetNearestTemp()
    {
        return nearestTemp;
    }
}
