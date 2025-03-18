using System.Collections;
using System.Collections.Generic;
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

    IEnumerator LoadSceneAsync(Scene scene)
    {
        loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync((int)scene);
        float targetProgress = 0;

        while (!operation.isDone)
        {
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            loadingBar.fillAmount = Mathf.Lerp(loadingBar.fillAmount, targetProgress, Time.deltaTime * 5);

            yield return null;
        }

        loadingBar.fillAmount = 1;

        loadingScreen.SetActive(false);
    }
}
