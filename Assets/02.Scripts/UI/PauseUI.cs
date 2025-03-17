using Unity.VisualScripting;
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

    [Header("Build Object Creator")]
    [SerializeField] private BuildObjectCreator creator;

    private bool isMuted = false;
    private float previousVolume = 1f;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnClickResumeButton);
        volumeSettingButton.onClick.AddListener(OnClickVolumeSettingButton);
        mainMenuButton.onClick.AddListener(OnClickMainMenuButton);
        exitButton.onClick.AddListener(OnClickExitButton);
        muteButton.onClick.AddListener(OnClickMuteButton);
        returnButton.onClick.AddListener(OnClickReturnButton);

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

    private void OnClickMainMenuButton()
    {
        Debug.Log("메인메뉴로 이동");
    }

    private void OnClickExitButton()
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
            if (!creator.IsPrevieObject())
                return; 

            OnPauseUI();
        }
    }
}
