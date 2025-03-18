using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Scene
{
    StartScene,
    MainScene
}

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image loadingBar;

    public void LoadScene(Scene scene)
    {
        StartCoroutine(LoadSceneAsync(scene));
    }

    /// <summary>
    /// 비동기로 씬 전환 및 로딩 화면 표시
    /// </summary>
    /// <param name="scene">이동할 씬</param>
    /// <returns></returns>
    private IEnumerator LoadSceneAsync(Scene scene)
    {
        loadingScreen.SetActive(true);

        // 비동기 씬 로드 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync((int)scene);
        float targetProgress = 0;

        // 씬 로드가 완료될 때까지 반복
        while (!operation.isDone)
        {
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // 현재 로드 진행률에 따라 이미지 fillAmount 변경
            loadingBar.fillAmount = Mathf.Lerp(loadingBar.fillAmount, targetProgress, Time.deltaTime * 5);

            yield return null;
        }

        loadingBar.fillAmount = 1;

        loadingScreen.SetActive(false);
    }
}
