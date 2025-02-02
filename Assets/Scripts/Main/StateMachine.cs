using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateMachine : MonoBehaviour
{
    [SerializeField] PauseSystem pauseSystem;
    [SerializeField] FoodGenerator generateFoodScript;

    [SerializeField] Animation foodMovesToTableFromLeft;
    [SerializeField] Animation foodMovesToTableFromMiddle;
    [SerializeField] Animation foodMovesToTableFromRight;

    [SerializeField] PlayerInput input;
    [SerializeField] FirstPersonLook mouseLook;
    [SerializeField] Animator firstPersonCameraAnimator;

    [SerializeField] AudioSource kitchenAudioSource;
    [SerializeField] List<AudioClip> kitchenSoundClips;
    [SerializeField] AudioClip foodFinishedClip;

    [SerializeField] Event kitchenCookingSoundsFinished;

    BasicFood chosenFood;
    public enum playerStateEnum
    {
        Sequence,
        Playable,
        OnlyMouse,
    }
    public playerStateEnum playerStateNow;

    public enum gameState
    {
        Cooking,
        FoodSelection,
        FoodChosen,
        FoodTakesEffect
    }

    private void Start()
    {
        if (SaveSystem.SearchIfFileExists("Game State") == true)
        {
            gameState gameState = (gameState)System.Enum.Parse(typeof(gameState), SaveSystem.LoadData<string>("Game State"));
            ChangeGameState(gameState);
        }
        else
        {
            //Maybe SpawnSequence and then gameState
            ChangeGameState(gameState.FoodSelection);
        }
    }

    public void ChangePlayerState(playerStateEnum playerState)
    {
        switch (playerState)
        {
            case playerStateEnum.Sequence:
                input.actions.FindAction("Look").Disable();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case playerStateEnum.Playable:
                input.actions.FindAction("Look").Enable();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case playerStateEnum.OnlyMouse:
                input.actions.FindAction("Look").Disable();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
        playerStateNow = playerState;
    }
    public void ChangeGameState(gameState state)
    {
        StartCoroutine(ChangeGameStateCoroutine(state));
    }

    public void FoodIsTaken(BasicFood foodScript)
    { 
        chosenFood = foodScript;
        ChangeGameState(gameState.FoodChosen);
    }
    private IEnumerator ChangeGameStateCoroutine(gameState state)
    {
        switch (state)
        {
            case gameState.FoodSelection:
                //Player can look (and move if possible)
                ChangePlayerState(playerStateEnum.Playable);

                //Let it seem like someone is cooking in the kitchen
                yield return StartCoroutine(PlayKitchenSounds(3));

                //Player is looking on the table in anticipation
                ChangePlayerState(playerStateEnum.Sequence);
                mouseLook.ResetLook();
                firstPersonCameraAnimator.SetTrigger("Look At Food");

                yield return new WaitForSeconds(1);

                ChangePlayerState(playerStateEnum.OnlyMouse);
                generateFoodScript.enabled = true;
                break;

            case gameState.FoodChosen:
                ChangePlayerState(playerStateEnum.Sequence);
                generateFoodScript.enabled = false;

                generateFoodScript.DestroyAllFoodsBut(chosenFood);

                MoveFoodToTableMiddle(firstPersonCameraAnimator.GetInteger("Chosen Food"));
                firstPersonCameraAnimator.SetTrigger("Set Back To Seat");

                yield return new WaitForSeconds(1);
                break;

            case gameState.FoodTakesEffect:
                SaveState(gameState.FoodTakesEffect);
                ChangePlayerState(playerStateEnum.Playable);

                generateFoodScript.enabled = false;
                // Activate Effects
                break;
        }
    }

    private void MoveFoodToTableMiddle(int LeftToRightAsint)
    {
        Animation animation = chosenFood.GetComponent<Animation>();
        if (animation == null)
        {
            animation = chosenFood.AddComponent<Animation>();
        }

        if (LeftToRightAsint == 1)
        {
            animation.clip = foodMovesToTableFromLeft.clip;
        }
        else if (LeftToRightAsint == 2)
        {
            animation.clip = foodMovesToTableFromMiddle.clip;
        }
        else
        {
            animation.clip = foodMovesToTableFromRight.clip;
        }

        animation.Play();
    }

    private IEnumerator PlayKitchenSounds(int countOfPlayingSounds)
    {
        List<AudioClip> sounds = new List<AudioClip>(kitchenSoundClips);

        for (int i = 0; i < countOfPlayingSounds; i++)
        {
            var generatedSound = GenerateRandomKitchenSound(sounds);
            sounds.Remove(generatedSound.clip);

            kitchenAudioSource.pitch = generatedSound.pitch;
            kitchenAudioSource.PlayOneShot(generatedSound.clip, PlayerPrefs.GetFloat("Volume"));

            // Warten, bis der Clip zu Ende ist und das Spiel nicht pausiert ist
            yield return WaitForAudioClip(kitchenAudioSource);
        }

        yield return new WaitForSeconds(1); // Hier ggf. `UnscaledTime` verwenden, falls Time.timeScale = 0

        kitchenAudioSource.PlayOneShot(foodFinishedClip, PlayerPrefs.GetFloat("Volume"));
        yield return WaitForAudioClip(kitchenAudioSource);

        // Coroutine abgeschlossen
        yield return null;
    }

    public void TogglePauseAudio(bool pause)
    {
        if (pause)
        {
            kitchenAudioSource.Pause();
        }
        else
        {
            kitchenAudioSource.UnPause();
        }
    }

    private IEnumerator WaitForAudioClip(AudioSource audioSource)
    {
        // Warte, solange der AudioClip spielt, oder das Spiel pausiert ist
        while (audioSource.isPlaying || IsGamePaused())
        {
            yield return null; // Ein Frame warten
        }
    }

    // Prüft, ob das Spiel pausiert ist (Time.timeScale == 0)
    private bool IsGamePaused()
    {
        return Time.timeScale == 0;
    }

    private (AudioClip clip, float pitch) GenerateRandomKitchenSound(List<AudioClip> emptyingList)
    {
        int index = UnityEngine.Random.Range(0, emptyingList.Count);
        float pitch = UnityEngine.Random.Range(0.5f, 1.5f);

        AudioClip chosenClip = emptyingList[index];
        return (chosenClip, pitch);
    }

    void SaveState(gameState state)
    {
        string stateToString = state.ToString();
        SaveSystem.SaveData(stateToString, "Game State");
    }
}
