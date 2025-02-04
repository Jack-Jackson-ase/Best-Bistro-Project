using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // 1. The Input System "using" statement

public class PauseSystem : MonoBehaviour
{
    [SerializeField] GameObject pauseScreen;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider sensitivitySlider;

    [SerializeField] StateMachine stateMachine;

    StateMachine.PlayerStateEnum oldPlayerState;

    [SerializeField] FirstPersonLook mouseLook;

    List<AudioSource> pausedAudioSources = new List<AudioSource>();
    public bool paused = false;
    bool isHold = false;

    // 2. This variable holds the stop action
    InputAction pauseAction;

    private void Start()
    {
        pauseAction = InputSystem.actions.FindAction("Pause Game");
        SetOptionsState();
    }
    void Update()
    {
        if (pauseAction.IsPressed() && isHold != true)
        {
            TogglePause();
            isHold = true;
        }
        else if (pauseAction.IsPressed() != true)
        {
            isHold = false;
        }
    }

    public void TogglePause()
    {
        //Continues Game
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            paused = false;

            stateMachine.TogglePause(paused);
            TogglePauseCanva();
        }

        //Pauses Game
        else
        {
            Time.timeScale = 0f;

            paused = true;

            TogglePauseCanva();
        }
        TogglePauseAudio(paused);
        stateMachine.TogglePause(paused);
    }
    void TogglePauseAudio(bool pause)
    {
        if (pause)
        {
            AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Pause();
                    pausedAudioSources.Add(audioSource);
                }
            }
        }
        else
        {
            foreach (AudioSource audioSource in pausedAudioSources)
            {
                audioSource.UnPause();
            }
        }
    }
    void TogglePauseCanva()
    {
        if (paused)
        {
            pauseScreen.SetActive(true);
        }
        else
        {
            pauseScreen.SetActive(false);
        }
    }

    public void ExitToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title Screen", LoadSceneMode.Single);
    }

    void SetOptionsState()
    {
        var options = SaveSystem.GetOptionsStats();

        volumeSlider.value = options.volume;
        sensitivitySlider.value = options.sensitivityRaw;
    }

    public void ChangeOptions()
    {
        PlayerPrefs.SetFloat("Mouse Sensitivity Raw", sensitivitySlider.value);
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);

        PlayerPrefs.Save();

        SetMouseSensitivity(sensitivitySlider.value * 20);
    }

    void SetMouseSensitivity(float sensitivity)
    {
        mouseLook.sensitivity = sensitivity;
    }
}
