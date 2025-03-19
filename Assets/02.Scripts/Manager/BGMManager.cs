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
    public float bgmFadeDuration = 1.5f; // 페이드 인/아웃 시간

    private Transform player;
    private bool isEnemyNearby = false;
    private WeatherManager.WeatherType currentWeather;
    private int enemyLayerMask;

    private float previousVolume;
    private bool isMuted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        player = PlayerManager.Instance.Player.transform;
        enemyLayerMask = LayerMask.GetMask("Enemy");
        if (environmentBgm == null || enemyBgm == null)
        {
            Debug.LogError("BGMManager: AudioSource가 설정되지 않았습니다!");
            return;
        }

        environmentBgm.loop = true;
        enemyBgm.loop = true;

        SetVolume(1f);

        WeatherManager.Instance.OnWeatherChanged += UpdateWeatherBGM;

        StartCoroutine(CheckForEnemies());

    }

    private void UpdateWeatherBGM()
    {
        if (isEnemyNearby) return; // 적이 있으면 날씨 BGM 변경 X

        currentWeather = WeatherManager.Instance.currentWeather;
        AudioClip newBGM = GetWeatherBGM(currentWeather);

        if (environmentBgm.clip != newBGM)
        {
            StartCoroutine(FadeInNewBGM(environmentBgm, newBGM));
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
            Collider[] hitColliders = Physics.OverlapSphere(player.position, enemyDetectionRadius, enemyLayerMask);
            bool enemyFound = hitColliders.Length > 0;

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
        StartCoroutine(FadeOutAndSwitch(environmentBgm, enemyBgm, enemyBGM));
    }

    private void StopEnemyBGM()
    {
        isEnemyNearby = false;
        StartCoroutine(FadeOutAndSwitch(enemyBgm, environmentBgm, GetWeatherBGM(currentWeather)));
    }


    private IEnumerator FadeOutAndSwitch(AudioSource from, AudioSource to, AudioClip newClip)
    {
        if (from.isPlaying)
        {
            yield return FadeOut(from);
            from.Stop();
        }

        to.clip = newClip;
        to.Play();
        yield return FadeIn(to);
    }

    private IEnumerator FadeInNewBGM(AudioSource source, AudioClip newClip)
    {
        if (source.isPlaying)
        {
            yield return FadeOut(source);
            source.Stop();
        }

        source.clip = newClip;
        source.Play();
        yield return FadeIn(source);
    }

    private IEnumerator FadeOut(AudioSource source)
    {
        float startVolume = source.volume;
        for (float t = 0; t < bgmFadeDuration; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0, t / bgmFadeDuration);
            yield return null;
        }
        source.volume = 0;
    }

    private IEnumerator FadeIn(AudioSource source)
    {
        float targetVolume = previousVolume;
        source.volume = 0;
        for (float t = 0; t < bgmFadeDuration; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(0, targetVolume, t / bgmFadeDuration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    public void SetVolume(float value)
    {
        previousVolume = value;
        if (!isMuted)
        {
            environmentBgm.volume = value;
            enemyBgm.volume = value;
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            previousVolume = environmentBgm.volume;
            environmentBgm.volume = 0f;
            enemyBgm.volume = 0f;
        }
        else
        {
            environmentBgm.volume = previousVolume;
            enemyBgm.volume = previousVolume;
        }
    }

    public bool IsMuted() => isMuted;

    public float GetCurrentVolume() => environmentBgm.volume;
}



