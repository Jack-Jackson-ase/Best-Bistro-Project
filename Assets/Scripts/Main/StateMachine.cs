using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StateMachine : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private PauseSystem pauseSystem;
    [SerializeField] private CookBehaviour cookBehindWallBehaviour;
    [SerializeField] private CookBehaviour cookNormalBehaviour;
    [SerializeField] private FoodGenerator generateFoodScript;
    [SerializeField] private PlayerStats playerStatsScript;
    [SerializeField] private Animator firstPersonCameraAnimator;
    [SerializeField] private FirstPersonLook mouseLook;
    [SerializeField] private MouseSelectOtherStuff selector;
    [SerializeField] private PlayerInput input;
    [SerializeField] private ChosenFoodUI foodUI;
    [SerializeField] private TextMeshProUGUI nextRoundText;
    [SerializeField] private RoundNumber roundNumberTextScript;
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private Image blackOverlay;
    [SerializeField] private AudioClip nextRoundSound;
    [SerializeField] private ParticleSystem coughParticle;

    [SerializeField] private AnimationClip foodMovesToTableFromLeft;
    [SerializeField] private AnimationClip foodMovesToTableFromMiddle;
    [SerializeField] private AnimationClip foodMovesToTableFromRight;
    [SerializeField] private AnimationClip eatFoodAnim;

    [SerializeField] private AudioSource kitchenAudioSource;
    [SerializeField] private List<AudioClip> kitchenSoundClips;
    [SerializeField] private AudioClip foodFinishedClip;
    [SerializeField] private AudioClip foodFinishedShortClip;

    private BasicFoodBehaviour takenFood;
    public BasicFoodBehaviour selectedFood;
    public int roundNumber;
    public PlayerStateEnum playerStateNow { get; private set; }
    private PlayerStateEnum oldPlayerState;

    public enum PlayerStateEnum { Sequence, Playable, OnlyMouse }
    public enum GameState { StartOfRound, FoodSelection, OneFoodSelected, FoodChosen, FoodTakesEffect, EndOfRound }

    void Start()
    {
        if (SaveSystem.SearchIfFileExists("Game State"))
        {
            SaveData loadedGame = SaveSystem.LoadSaveGameState("Game State");
            SetAllStats(loadedGame.hunger, loadedGame.health, loadedGame.roundNumber);
            GameState gameState = (GameState)System.Enum.Parse(typeof(GameState), loadedGame.gameState);
            ChangeGameState(gameState);
        }
        else
        {
            SetAllStats(50, 100, 1);
            ChangeGameState(GameState.StartOfRound);
        }
    }

    void SetAllStats(int hunger, int health, int roundNumber)
    {
        playerStatsScript.SetPlayerStats(hunger, health);
        this.roundNumber = roundNumber;
    }

    void SaveGameState(GameState gameState)
    {
        string enumString = gameState.ToString();
        SaveSystem.SaveGameState("Game State", enumString, roundNumber, playerStatsScript.hunger, playerStatsScript.health);
    }

    public void TogglePause(bool isPausing)
    {
        if (isPausing)
        {
            oldPlayerState = playerStateNow;
            ChangePlayerState(PlayerStateEnum.OnlyMouse);
        }
        else
        {
            ChangePlayerState(oldPlayerState);
        }
    }

    void ChangePlayerState(PlayerStateEnum playerState)
    {
        if (playerState == PlayerStateEnum.Sequence)
        {
            mouseLook.ResetLook();
            input.actions.Disable(); // Deakctivate all input
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (playerState == PlayerStateEnum.Playable)
        {
            input.actions.Enable(); // Activates all input
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (playerState == PlayerStateEnum.OnlyMouse)
        {
            input.actions.Disable(); // Deactivates everything...
            input.actions["Look"].Disable(); // ...but MouseControl
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        playerStateNow = playerState; // Saves active state
    }

    private void ChangeGameState(GameState state)
    {
        StopAllCoroutines();
        Debug.Log("Game State is changed to " + state);
        StartCoroutine(ChangeGameStateCoroutine(state));
    }
    public void OneFoodIsSelected(BasicFoodBehaviour food)
    {
        Debug.Log("Food is " + food);
        selectedFood = food;
        StartCoroutine(ChangeGameStateCoroutine(GameState.OneFoodSelected));
    }
    public void FoodIsTaken()
    {
        takenFood = selectedFood;
        ChangeGameState(GameState.FoodChosen);
    }

    private IEnumerator ChangeGameStateCoroutine(GameState state)
    {
        switch (state)
        {
            case GameState.StartOfRound:
                yield return HandleStartOfRoundState();
                break;

            case GameState.FoodSelection:
                HandleFoodSelectionState();
                break;

            case GameState.OneFoodSelected:
                yield return OneFoodIsSelected();
                break;

            case GameState.FoodChosen:
                yield return HandleTakenFoodState();
                break;

            case GameState.FoodTakesEffect:
                yield return HandleFoodTakesEffectState();
                ChangeGameState(GameState.EndOfRound);
                break;

            case GameState.EndOfRound:
                SaveGameState(GameState.EndOfRound);
                yield return HandleEndOfRoundState();

                if (playerStatsScript.health == 0)
                {
                    StartCoroutine(PlayerDeath());
                    break;
                }
                ChangeGameState(GameState.StartOfRound);
                break;
        }
    }




    private IEnumerator HandleStartOfRoundState()
    {
        ClearFoodData();
        StartCoroutine(ShowTextRoundNumber());

        ChangePlayerState(PlayerStateEnum.Playable);
        if (15 < roundNumber)
        {
            yield return PlayKitchenSounds(0, foodFinishedShortClip);
        }
        else if (10 < roundNumber)
        {
            yield return PlayKitchenSounds(1, foodFinishedShortClip);
        }
        else if (5 < roundNumber)
        {
            yield return PlayKitchenSounds(2, foodFinishedClip);
        }
        else
        {
            yield return PlayKitchenSounds(3, foodFinishedClip);
        }

        ChangePlayerState(PlayerStateEnum.Sequence);
        mouseLook.ResetLook();
        firstPersonCameraAnimator.SetTrigger("Look At Food");

        yield return new WaitForSeconds(1);
        generateFoodScript.enabled = true;
        ChangeGameState(GameState.FoodSelection);
    }
    private IEnumerator HandleEndOfRoundState()
    {
        Debug.Log("New Round is going to be started");
        CleanupOldPlates();
        IncreaseRoundNumber();
        yield return null;
    }

    private void IncreaseRoundNumber()
    {
        roundNumber++;
        playerStatsScript.NextRound(roundNumber);
        roundNumberTextScript.ShowRoundNumber();
        if (roundNumber > 50)
        {
            cookNormalBehaviour.CookIsJustStanding();
        }
    }
    void ClearFoodData()
    {
        selectedFood = null;
        takenFood = null;
        firstPersonCameraAnimator.SetInteger("Chosen Food", 0);
    }

    private void HandleFoodSelectionState()
    {
        Debug.Log("Food can be selected now");
        ChangePlayerState(PlayerStateEnum.OnlyMouse);
        ClearFoodData();
        selector.ClearSelectedFood();
        selector.Activate();
    }

    private IEnumerator OneFoodIsSelected()
    {
        Debug.Log("OneFoodIsSelected is triggered");
        selectedFood.IsSelected(true);
        selector.Deactivate();
        yield return MoveCameraToFoodAndActivateFoodUI();
        selector.Activate();
    }
    public void DeselectFood()
    {
        selector.Deactivate();
        selectedFood.IsSelected(false);
        selectedFood = null;
        firstPersonCameraAnimator.SetInteger("Chosen Food", 0);

        Debug.Log("Came to point with wait for seconds");
        StartCoroutine(WaitThenChangeGameState(GameState.FoodSelection));
    }

    IEnumerator WaitThenChangeGameState(GameState gameState)
    {
        yield return new WaitForSeconds(0.4f);

        ChangeGameState(gameState);
    }
    private IEnumerator MoveCameraToFoodAndActivateFoodUI()
    {
        Debug.Log("Animator should now play animation with food at position " + selectedFood.foodNumber);
        firstPersonCameraAnimator.SetInteger("Chosen Food", selectedFood.foodNumber);
        yield return new WaitForSeconds(1f);

        foodUI.gameObject.SetActive(true);
        if (selectedFood != null)
        {
            foodUI.ActivateUIForChosenFood(selectedFood);
        }
    }

    private IEnumerator HandleTakenFoodState()
    {
        Debug.Log("Food is chosen in State Machine at least");
        ChangePlayerState(PlayerStateEnum.Sequence);
        generateFoodScript.DestroyAllFoodsBut(takenFood);
        generateFoodScript.enabled = false;

        firstPersonCameraAnimator.SetTrigger("Set Back to Seat");
        MoveFoodToTableMiddle(firstPersonCameraAnimator.GetInteger("Chosen Food"));
        foodUI.gameObject.SetActive(false);
        selector.Deactivate();

        Debug.Log("Eat Food Animations started");
        yield return new WaitForSeconds(1);

        firstPersonCameraAnimator.SetTrigger("Eat");
        if (roundNumber == 4) { cookBehindWallBehaviour.CookIsPeeking(); }

        yield return new WaitForSeconds(1);

        takenFood.AddComponent<Animation>().AddClip(eatFoodAnim, "EatFood");
        takenFood.GetComponent<Animation>().Play("EatFood");
        yield return new WaitForSeconds(1);

        HideObjectWithoutDisabling(takenFood.gameObject);
        selectedFood.ActivateChosenFood(playerStatsScript);
    }

    private IEnumerator HandleFoodTakesEffectState()
    {
        Debug.Log("Food is taking effect in State Machine at least");
        ChangePlayerState(PlayerStateEnum.Playable);

        takenFood.ActivateChosenFood(playerStatsScript);
        yield return new WaitForSeconds(3);
    }
    public void FoodHasFinishedTakingEffect()
    {
        ChangeGameState(GameState.EndOfRound);
    }

    public void SpitBlood()
    {
        coughParticle.Play();
    }

    private void CleanupOldPlates()
    {
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("plate"))
        {
            Destroy(plate.transform.parent.gameObject);
        }
    }

    void HideObjectWithoutDisabling(GameObject obj)
    {
        obj.transform.localScale = Vector3.zero;
    }

    private IEnumerator ShowTextRoundNumber()
    {
        if (roundNumber >= 5)
        {
            nextRoundText.color = Color.red;
            audioSource.PlayOneShot(nextRoundSound, 0.5f);
        }
        nextRoundText.text = $"Round {roundNumber}";
        yield return new WaitForSeconds(3);
        nextRoundText.text = string.Empty;
    }

    private void MoveFoodToTableMiddle(int positionIndex)
    {
        Animation animation = takenFood.transform.parent.GetComponent<Animation>() ?? takenFood.transform.parent.gameObject.AddComponent<Animation>();

        AnimationClip clip = positionIndex switch
        {
            1 => foodMovesToTableFromLeft,
            2 => foodMovesToTableFromMiddle,
            _ => foodMovesToTableFromRight
        };

        animation.AddClip(clip, "MoveFood");
        animation.Play("MoveFood");
    }

    private IEnumerator PlayKitchenSounds(int countOfPlayingSounds, AudioClip foodFinishedPling)
    {
        List<AudioClip> sounds = new List<AudioClip>(kitchenSoundClips);

        for (int i = 0; i < countOfPlayingSounds; i++)
        {
            var clip = GenerateRandomKitchenSound(sounds);
            sounds.Remove(clip);

            kitchenAudioSource.PlayOneShot(clip);
            yield return new WaitWhile(() => kitchenAudioSource.isPlaying || IsGamePaused());
        }

        kitchenAudioSource.PlayOneShot(foodFinishedPling);
        yield return new WaitWhile(() => kitchenAudioSource.isPlaying || IsGamePaused());
    }

    private bool IsGamePaused() => Time.timeScale == 0;

    private AudioClip GenerateRandomKitchenSound(List<AudioClip> availableClips)
    {
        int index = Random.Range(0, availableClips.Count);
        return availableClips[index];
    }

    private IEnumerator PlayerDeath()
    {
        firstPersonCameraAnimator.SetTrigger("Player Death");
        cookNormalBehaviour.CookIsWatchingDeath();
        blackOverlay.gameObject.SetActive(true);

        while (blackOverlay.color.a < 1.0f)
        {
            blackOverlay.color = new Color(blackOverlay.color.r, blackOverlay.color.g, blackOverlay.color.b, blackOverlay.color.a + (Time.deltaTime * 0.2f));
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        ChangePlayerState(PlayerStateEnum.OnlyMouse);
        deathMenu.SetActive(true);

        SaveSystem.DeleteSaveGame("Game State");
    }

    public void RestartGame()
    {
        SaveSystem.DeleteSaveGame("Game State");

        SceneManager.UnloadSceneAsync(1);
        SceneManager.LoadScene(1);
    }
}
