using TMPro;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private Action confirmAction;

    private void Start()
    {
        // PauseUI 버튼 이벤트 등록
        resumeButton.onClick.AddListener(OnClickResumeButton);
        volumeSettingButton.onClick.AddListener(OnClickVolumeSettingButton);
        mainMenuButton.onClick.AddListener(() => ShowConfirmUI("메인 메뉴로 이동하시겠습니까?", "메인 메뉴", OnConfirmMainMenu));
        exitButton.onClick.AddListener(() => ShowConfirmUI("게임을 종료하시겠습니까?", "게임 종료", OnConfirmExit));

        // ConfirmUI 버튼 이벤트 등록
        returnButton.onClick.AddListener(OnClickReturnButton);
        cancelButton.onClick.AddListener(HideConfirmUI);

        // VolumeSettingUI 슬라이더 이벤트 등록
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        muteButton.onClick.AddListener(OnClickMuteButton);

        // GameOverUI 버튼 이벤트 등록
        gameOver_mainMenuButton.onClick.AddListener(() => ShowConfirmUI("메인 메뉴로 이동하시겠습니까?", "메인 메뉴", OnConfirmMainMenu));
        gameOver_exitButton.onClick.AddListener(() => ShowConfirmUI("게임을 종료하시겠습니까?", "게임 종료", OnConfirmExit));

        // 볼륨 초기화
        volumeSlider.value = BGMManager.Instance.GetCurrentVolume();
        muteCheckImage.enabled = BGMManager.Instance.IsMuted();


        PlayerManager.Instance.Player.condition.gameMenuUI = this;
    }

    // PauseUI 실행 함수
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

        volumeSlider.value = BGMManager.Instance.GetCurrentVolume();
    }

    private void OnClickMuteButton()
    {
        BGMManager.Instance.ToggleMute();
        muteCheckImage.enabled = BGMManager.Instance.IsMuted();
    }

    private void OnVolumeSliderChanged(float value)
    {
        if (!BGMManager.Instance.IsMuted())
            BGMManager.Instance.SetVolume(value);
    }

    private void OnClickReturnButton()
    {
        mainUI.SetActive(true);
        volumeSettingUI.SetActive(false);
    }

    private void ShowConfirmUI(string message, string confirmText, Action action)
    {
        // 게임종료, 메인메뉴에 따라 액션에 함수 등록, noticeText 및 buttonText 텍스트 변경
        confirmAction = action;
        noticeText.text = message;
        confirmButtonText.text = confirmText;

        confirmUI.SetActive(true);

        // ConfirmUI 버튼 제외 모든 버튼 비활성화
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
        SceneManager.LoadScene((int)Scene.StartScene);
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
