using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateMachine : MonoBehaviour
{
    [SerializeField] PauseSystem pauseSystem;
    [SerializeField] FoodGenerator generateFoodScript;
    [SerializeField] PlayerStats playerStatsScript;

    [SerializeField] AnimationClip foodMovesToTableFromLeft;
    [SerializeField] AnimationClip foodMovesToTableFromMiddle;
    [SerializeField] AnimationClip foodMovesToTableFromRight;

    [SerializeField] AnimationClip eatFoodAnim;

    [SerializeField] PlayerInput input;
    [SerializeField] FirstPersonLook mouseLook;
    [SerializeField] Animator firstPersonCameraAnimator;

    [SerializeField] AudioSource kitchenAudioSource;
    [SerializeField] List<AudioClip> kitchenSoundClips;
    [SerializeField] AudioClip foodFinishedClip;

    [SerializeField] Event kitchenCookingSoundsFinished;

    BasicFood chosenFood;
    [SerializeField] ChosenFoodUI foodUI;
    [SerializeField] TextMeshProUGUI nextRoundText;

    int roundNumber;
    public playerStateEnum playerStateNow;

    public enum playerStateEnum
    {
        Sequence,
        Playable,
        OnlyMouse,
    }

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
            //gameState gameState = (gameState)System.Enum.Parse(typeof(gameState), SaveSystem.LoadData<string>("Game State"));
            //Here Would the game state be selected,but for the start we stay at Food Selection
            //ChangeGameState(gameState);
            ChangeGameState(gameState.FoodSelection);
        }
        else
        {
            roundNumber = 1;
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

                //Check if there are plates left from last round
                GameObject[] oldPlates = GameObject.FindGameObjectsWithTag("plate");
                foreach (GameObject plate in oldPlates)
                {
                    Destroy(plate.transform.parent.gameObject);
                }

                //Show which round it is
                roundNumber += 1;
                StartCoroutine(ShowTextRoundNumber());

                //Player can look (and move if possible)
                ChangePlayerState(playerStateEnum.Playable);

                //Let it seem like someone is cooking in the kitchen
                yield return StartCoroutine(PlayKitchenSounds(3));

                //Player is looking on the table in anticipation
                ChangePlayerState(playerStateEnum.Sequence);
                mouseLook.ResetLook();
                firstPersonCameraAnimator.SetTrigger("Look At Food");

                yield return new WaitForSeconds(1);

                //Start the food generating Process
                ChangePlayerState(playerStateEnum.OnlyMouse);
                generateFoodScript.enabled = true;
                break;

            case gameState.FoodChosen:
                //The food is chosen, so destroy other foods
                ChangePlayerState(playerStateEnum.Sequence);

                generateFoodScript.DestroyAllFoodsBut(chosenFood);
                generateFoodScript.enabled = false;

                //Deactivates Food UI for food and sets camera back to seat
                firstPersonCameraAnimator.SetTrigger("Set Back to Seat");
                MoveFoodToTableMiddle(firstPersonCameraAnimator.GetInteger("Chosen Food"));
                foodUI.DeactivateChosenFood();

                yield return new WaitForSeconds(1);
                //Activates Eat Sequence
                firstPersonCameraAnimator.SetTrigger("Eat");

                yield return new WaitForSeconds(1);

                ChangePlayerState(playerStateEnum.Sequence);
                chosenFood.AddComponent<Animation>().AddClip(eatFoodAnim, "EatFood");
                chosenFood.GetComponent<Animation>().Play("EatFood");

                yield return new WaitForSeconds(1);

                //Food is not seeable
                chosenFood.gameObject.GetComponent<MeshRenderer>().enabled = false;
                ChangeGameState(gameState.FoodTakesEffect);
                break;

            case gameState.FoodTakesEffect:
                //Saves so if the player tries to go back, he cant
                SaveState(gameState.FoodTakesEffect);
                ChangePlayerState(playerStateEnum.Playable);

                //Activates the foods properties (and if implemented, special Effects too)
                generateFoodScript.enabled = false;
                chosenFood.ActivateChosenFood(playerStatsScript);

                yield return new WaitForSeconds(3);

                //Takes some hunger away, or if hunger is 0, some health is taken away
                playerStatsScript.NextRound();

                ChangeGameState(gameState.FoodSelection);
                break;
        }
    }

    IEnumerator ShowTextRoundNumber()
    {
        nextRoundText.text = "Round " + roundNumber;
        yield return new WaitForSeconds(3);
        nextRoundText.text = null;
    }

    private void MoveFoodToTableMiddle(int LeftToRightAsint)
    {
        Animation animation = chosenFood.transform.parent.GetComponent<Animation>();
        if (animation == null)
        {
            animation = chosenFood.transform.parent.AddComponent<Animation>();
        }

        if (LeftToRightAsint == 1)
        {
            animation.AddClip(foodMovesToTableFromLeft, "MoveFood");
        }
        else if (LeftToRightAsint == 2)
        {
            animation.AddClip(foodMovesToTableFromMiddle, "MoveFood");
        }
        else
        {
            animation.AddClip(foodMovesToTableFromRight, "MoveFood");
        }

        animation.Play("MoveFood");
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
