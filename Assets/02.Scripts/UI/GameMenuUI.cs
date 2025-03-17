using TMPro;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;

    [Header("Pause UI Main")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button volumeSettingButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitButton;

    [Header("Volume Setting UI")]
    [SerializeField] private GameObject volumeSettingUI;
    [SerializeField] private Button muteButton;
    [SerializeField] private Image muteCheckImage;
    [SerializeField] private Button returnButton;
    [SerializeField] private Slider volumeSlider;

    [Header("Confirm UI")]
    [SerializeField] private GameObject confirmUI;
    [SerializeField] private TextMeshProUGUI noticeText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private Button cancelButton;

    [Header("GameOver UI")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Button gameOver_mainMenuButton;
    [SerializeField] private Button gameOver_exitButton;

    [Header("Build Object Creator")]
    [SerializeField] private BuildObjectCreator creator;

    private bool isMuted = false;
    private float previousVolume = 1f;

    private Action confirmAction;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnClickResumeButton);
        volumeSettingButton.onClick.AddListener(OnClickVolumeSettingButton);
        mainMenuButton.onClick.AddListener(() => ShowConfirmUI("메인 메뉴로 이동하시겠습니까?", "메인 메뉴", OnConfirmMainMenu));
        exitButton.onClick.AddListener(() => ShowConfirmUI("게임을 종료하시겠습니까?", "게임 종료", OnConfirmExit));

        muteButton.onClick.AddListener(OnClickMuteButton);
        returnButton.onClick.AddListener(OnClickReturnButton);

        cancelButton.onClick.AddListener(HideConfirmUI);

        volumeSlider.onValueChanged.AddListener(SetVolume);

        gameOver_mainMenuButton.onClick.AddListener(() => ShowConfirmUI("메인 메뉴로 이동하시겠습니까?", "메인 메뉴", OnConfirmMainMenu));
        gameOver_exitButton.onClick.AddListener(() => ShowConfirmUI("게임을 종료하시겠습니까?", "게임 종료", OnConfirmExit));

        volumeSlider.value = BGMManager.Instance.environmentBgm.volume;
    }
    public void OnPauseUI()
    {
        pauseUI.SetActive(true);
        PlayerManager.Instance.Player.controller.canLook = false;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    private void OnClickResumeButton()
    {
        PlayerManager.Instance.Player.controller.canLook = true;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        pauseUI.SetActive(false);
    }

    private void OnClickVolumeSettingButton()
    {
        mainUI.SetActive(false);
        volumeSettingUI.SetActive(true);
    }

    private void OnClickMuteButton()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            previousVolume = BGMManager.Instance.environmentBgm.volume;
            BGMManager.Instance.environmentBgm.volume = 0f;
            BGMManager.Instance.enemyBgm.volume = 0f;
            muteCheckImage.enabled = true;
        }
        else
        {
            BGMManager.Instance.environmentBgm.volume = previousVolume;
            BGMManager.Instance.enemyBgm.volume = previousVolume;
            muteCheckImage.enabled = false;
        }
    }

    public void SetVolume(float value)
    {
        if (!isMuted)
        {
            BGMManager.Instance.environmentBgm.volume = value;
            BGMManager.Instance.enemyBgm.volume = value;
            previousVolume = value;
        }
    }

    private void OnClickReturnButton()
    {
        mainUI.SetActive(true);
        volumeSettingUI.SetActive(false);
    }

    private void ShowConfirmUI(string message, string confirmText, Action action)
    {
        confirmAction = action;
        noticeText.text = message;
        confirmButtonText.text = confirmText;

        confirmUI.SetActive(true);

        DisableAllButtons();

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirmAction?.Invoke(); 
            HideConfirmUI();
        });
    }

    private void HideConfirmUI()
    {
        confirmUI.SetActive(false);
        EnableAllButtons();
    }

    public void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
        PlayerManager.Instance.Player.controller.canLook = false;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
    }

    private void OnConfirmMainMenu()
    {
        Debug.Log("메인 메뉴로 이동합니다.");
        Time.timeScale = 1f;
    }

    private void OnConfirmExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
   Application.Quit();
#endif
    }

    private void DisableAllButtons()
    {
        resumeButton.interactable = false;
        volumeSettingButton.interactable = false;
        mainMenuButton.interactable = false;
        exitButton.interactable = false;

        if (gameOverUI.activeSelf)
        {
            gameOver_mainMenuButton.interactable = false;
            gameOver_exitButton.interactable = false;
        }
    }
    private void EnableAllButtons()
    {
        resumeButton.interactable = true;
        volumeSettingButton.interactable = true;
        mainMenuButton.interactable = true;
        exitButton.interactable = true;

        if (gameOverUI.activeSelf)
        {
            gameOver_mainMenuButton.interactable = true;
            gameOver_exitButton.interactable = true;
        }
    }

    public void OnPauseUIInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            OnPauseUI();
        }
    }
}
