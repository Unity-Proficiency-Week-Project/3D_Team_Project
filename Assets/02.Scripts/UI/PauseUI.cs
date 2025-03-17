using TMPro;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [Header("Pause UI Main")]
    [SerializeField] private GameObject pauseUI;
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
    [SerializeField] private TextMeshProUGUI confrimButtonText;
    [SerializeField] private Button cancelButton;

    [Header("Build Object Creator")]
    [SerializeField] private BuildObjectCreator creator;

    private bool isMuted = false;
    private float previousVolume = 1f;

    private Action confirmAction;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnClickResumeButton);
        volumeSettingButton.onClick.AddListener(OnClickVolumeSettingButton);
        mainMenuButton.onClick.AddListener(() => ShowConfirmUI("메인 메뉴로 이동하시겠습니까?", "메인 메뉴",OnConfirmMainMenu));
        exitButton.onClick.AddListener(() => ShowConfirmUI("게임을 종료하시겠습니까?", "게임 종료", OnConfirmExit));

        muteButton.onClick.AddListener(OnClickMuteButton);
        returnButton.onClick.AddListener(OnClickReturnButton);

        cancelButton.onClick.AddListener(HideConfirmUI);

        volumeSlider.onValueChanged.AddListener(SetVolume);

        volumeSlider.value = BGMManager.Instance.enemyBgm.volume;
        volumeSlider.value = BGMManager.Instance.environmentBgm.volume;
    }

    public void OnPauseUI()
    {
        gameObject.SetActive(true);
        PlayerManager.Instance.Player.controller.canLook = false;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    private void OnClickMuteButton()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            previousVolume = BGMManager.Instance.enemyBgm.volume;
            BGMManager.Instance.enemyBgm.volume = 0f;
            BGMManager.Instance.environmentBgm.volume = 0f;
            muteCheckImage.enabled = true;
        }
        else
        {
            BGMManager.Instance.enemyBgm.volume = previousVolume;
            BGMManager.Instance.environmentBgm.volume = previousVolume;
            muteCheckImage.enabled = false;
        }
    }

    public void SetVolume(float value)
    {
        if (!isMuted)
        {
            BGMManager.Instance.enemyBgm.volume = value;
            BGMManager.Instance.environmentBgm.volume = value;
        }
        previousVolume = value;
    }

    private void OnClickReturnButton()
    {
        pauseUI.SetActive(true);
        volumeSettingUI.SetActive(false);
    }

    private void OnClickResumeButton()
    {
        PlayerManager.Instance.Player.controller.canLook = true;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private void OnClickVolumeSettingButton()
    {
        pauseUI.SetActive(false);
        volumeSettingUI.SetActive(true);
    }

    private void ShowConfirmUI(string message, string confirmText, Action action)
    {
        confirmAction = action;
        noticeText.text = message;
        confrimButtonText.text = confirmText;

        confirmUI.SetActive(true);

        resumeButton.interactable = false;
        volumeSettingButton.interactable = false;
        mainMenuButton.interactable = false;
        exitButton.interactable = false;

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
        pauseUI.SetActive(true);

        resumeButton.interactable = true;
        volumeSettingButton.interactable = true;
        mainMenuButton.interactable = true;
        exitButton.interactable = true;
    }

    private void OnConfirmMainMenu()
    {
        Debug.Log("메인 메뉴로 이동합니다.");
    }

    private void OnConfirmExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnPauseUIInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            OnPauseUI();
        }
    }
}
