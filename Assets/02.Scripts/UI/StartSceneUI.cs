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

    private void Start()
    {
        startButton.onClick.AddListener(OnClickStartButton);
        helpButton.onClick.AddListener(OnClickHelpButton);
        exitButton.onClick.AddListener(OnClickExitButton);
    }

    private void OnClickStartButton()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void OnClickHelpButton()
    {
        helpPannel.SetActive(true);
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
