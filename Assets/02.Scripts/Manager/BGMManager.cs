using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("Audio Source")]
    public AudioSource environmentBgm;
    public AudioSource enemyBgm;

    [Header("Weather BGMs")]
    public AudioClip normalBGM;
    public AudioClip rainyBGM;
    public AudioClip snowyBGM;

    [Header("Enemy BGM")]
    public AudioClip enemyBGM;

    [Header("Settings")]
    public float enemyDetectionRadius = 20f; // 적 감지 거리

    private Transform player;
    private bool isEnemyNearby = false;
    private WeatherManager.WeatherType currentWeather;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        player = PlayerManager.Instance.Player.transform;
        WeatherManager.Instance.OnWeatherChanged += UpdateWeatherBGM;
        UpdateWeatherBGM();
        StartCoroutine(CheckForEnemies());
    }

    private void UpdateWeatherBGM()
    {
        if (isEnemyNearby) return; // 적이 있으면 날씨 BGM 변경 X

        currentWeather = WeatherManager.Instance.currentWeather;
        AudioClip newBGM = GetWeatherBGM(currentWeather);

        if (environmentBgm.clip != newBGM)
        {
            environmentBgm.clip = newBGM;
            environmentBgm.Play();
        }
    }
    private AudioClip GetWeatherBGM(WeatherManager.WeatherType weather)
    {
        switch (weather)
        {
            case WeatherManager.WeatherType.Sunny: return normalBGM;
            case WeatherManager.WeatherType.Rainy: return rainyBGM;
            case WeatherManager.WeatherType.Snowy: return snowyBGM;
            default: return normalBGM; // 기본값
        }
    }

    private IEnumerator CheckForEnemies()
    {
        while (true)
        {
            Collider[] hitColliders = Physics.OverlapSphere(player.position, enemyDetectionRadius);
            bool enemyFound = false;

            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    enemyFound = true;
                    break;
                }
            }

            if (enemyFound && !isEnemyNearby)
            {
                PlayEnemyBGM();
            }
            else if (!enemyFound && isEnemyNearby)
            {
                StopEnemyBGM();
            }

            yield return new WaitForSeconds(1f); // 1초마다 검사
        }
    }

    private void PlayEnemyBGM()
    {
        isEnemyNearby = true;
        environmentBgm.Pause();
        enemyBgm.clip = enemyBGM;
        enemyBgm.Play();
    }

    private void StopEnemyBGM()
    {
        isEnemyNearby = false;
        enemyBgm.Stop();
        environmentBgm.Play();
    }
}
