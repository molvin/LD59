using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEditor;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public bool Open;

    public GameObject Root;

    public Slider MasterSlider;
    public Slider SFXSlider;
    public Slider MusicSlider;
    public TMP_Dropdown FullscreenDropdown;
    public TMP_Dropdown ResolutionDropdown;
    public Button UnstuckButton;
    public Button QuitButton;

    public string MasterBusPath;
    public string SFXBusPath;
    public string MusicBusPath;

    private Bus masterBus;
    private Bus sfxBus;
    private Bus musicBus;

    private Resolution[] resolutions;

    private void Start()
    {
        Root.SetActive(false);

        MasterSlider.onValueChanged.AddListener(SetMasterVolume);
        SFXSlider.onValueChanged.AddListener(SetSFXVolume);
        MusicSlider.onValueChanged.AddListener(SetMusicVolume);

        FullscreenDropdown.ClearOptions();
        FullscreenDropdown.AddOptions(new List<string> { "Fullscreen", "Windowed", "Borderless" });
        FullscreenDropdown.value = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? 0
            : Screen.fullScreenMode == FullScreenMode.Windowed ? 1 : 2;
        FullscreenDropdown.onValueChanged.AddListener(SetFullscreen);

        resolutions = Screen.resolutions;
        var resOptions = new List<string>();
        int currentIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            resOptions.Add($"{resolutions[i].width}x{resolutions[i].height} {resolutions[i].refreshRateRatio.numerator}Hz");
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentIndex = i;
        }
        ResolutionDropdown.ClearOptions();
        ResolutionDropdown.AddOptions(resOptions);
        ResolutionDropdown.value = currentIndex;
        ResolutionDropdown.onValueChanged.AddListener(SetResolution);

        UnstuckButton.onClick.AddListener(Unstuck);
        QuitButton.onClick.AddListener(Quit);

        masterBus = FMODUnity.RuntimeManager.GetBus(MasterBusPath);
        sfxBus = FMODUnity.RuntimeManager.GetBus(SFXBusPath);
        musicBus = FMODUnity.RuntimeManager.GetBus(MusicBusPath);

        // TODO: set the sliders to the values of the busses
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Open = !Open;

            GameManager.Get().Player.MovementEnabled = !Open;
            Cursor.lockState = Open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = Open;

            Root.SetActive(Open);
        }
    }

    private void SetMasterVolume(float value) => masterBus.setVolume(value);
    private void SetSFXVolume(float value) => sfxBus.setVolume(value);
    private void SetMusicVolume(float value) => musicBus.setVolume(value);

    private void SetFullscreen(int index)
    {
        FullScreenMode mode = index == 0 ? FullScreenMode.ExclusiveFullScreen
            : index == 1 ? FullScreenMode.Windowed
            : FullScreenMode.FullScreenWindow;
        Screen.fullScreenMode = mode;
    }

    private void SetResolution(int index)
    {
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);
    }

    private void Unstuck()
    {
        // TODO: this button should only be interactable if this is true: Time.time - player.lastTimeStandingOnBoat > 120.0f
        GameManager.Get().Player.Unstuck();
    }

    private void Quit()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
