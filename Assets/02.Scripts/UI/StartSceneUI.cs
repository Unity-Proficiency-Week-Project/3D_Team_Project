using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject helpPannel;

    [SerializeField] private LoadingScreen loadingScreen;

    private void Start()
    {
        startButton.onClick.AddListener(OnClickStartButton);
        helpButton.onClick.AddListener(OnClickHelpButton);
        exitButton.onClick.AddListener(OnClickExitButton);
    }

    private void OnClickStartButton()
    {
        loadingScreen.LoadScene(Scene.MainScene);
    }

    private void OnClickHelpButton()
    {
        helpPannel.SetActive(!helpPannel.activeSelf);
    }

    private void OnClickExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
   Application.Quit();
#endif
    }
}
