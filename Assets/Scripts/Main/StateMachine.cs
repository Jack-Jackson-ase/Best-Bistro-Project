using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateMachine : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private PauseSystem pauseSystem;
    [SerializeField] private FoodGenerator generateFoodScript;
    [SerializeField] private PlayerStats playerStatsScript;
    [SerializeField] private Animator firstPersonCameraAnimator;
    [SerializeField] private FirstPersonLook mouseLook;
    [SerializeField] private MouseSelectOtherStuff selector;
    [SerializeField] private PlayerInput input;
    [SerializeField] private ChosenFoodUI foodUI;
    [SerializeField] private TextMeshProUGUI nextRoundText;
    [SerializeField] private AudioClip nextRoundSound;
    [SerializeField] private ParticleSystem coughParticle;

    [SerializeField] private AnimationClip foodMovesToTableFromLeft;
    [SerializeField] private AnimationClip foodMovesToTableFromMiddle;
    [SerializeField] private AnimationClip foodMovesToTableFromRight;
    [SerializeField] private AnimationClip eatFoodAnim;

    [SerializeField] private AudioSource kitchenAudioSource;
    [SerializeField] private List<AudioClip> kitchenSoundClips;
    [SerializeField] private AudioClip foodFinishedClip;

    private BasicFoodBehaviour takenFood;
    public BasicFoodBehaviour selectedFood;
    public int roundNumber;
    public PlayerStateEnum playerStateNow { get; private set; }
    private PlayerStateEnum oldPlayerState;

    public enum PlayerStateEnum { Sequence, Playable, OnlyMouse }
    public enum GameState { StartOfRound, FoodSelection, OneFoodSelected, FoodChosen, FoodTakesEffect, EndOfRound }

    private void Start()
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
        mouseLook.ResetLook();
        if (playerState == PlayerStateEnum.Sequence)
        {
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
                ChangeGameState(GameState.StartOfRound);
                break;
        }
    }

    private IEnumerator HandleStartOfRoundState()
    {
        ClearFoodData();
        StartCoroutine(ShowTextRoundNumber());

        ChangePlayerState(PlayerStateEnum.Playable);
        yield return PlayKitchenSounds(3);

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
        playerStatsScript.NextRound();
        CleanupOldPlates();
        roundNumber++;
        yield return null;
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
        if (roundNumber > 10)
        {
        nextRoundText.color = Color.red;
        audioSource.PlayOneShot(nextRoundSound, PlayerPrefs.GetFloat("Volume") * 0.5f);
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

    private IEnumerator PlayKitchenSounds(int countOfPlayingSounds)
    {
        List<AudioClip> sounds = new List<AudioClip>(kitchenSoundClips);

        for (int i = 0; i < countOfPlayingSounds; i++)
        {
            var clip = GenerateRandomKitchenSound(sounds);
            sounds.Remove(clip);

            kitchenAudioSource.PlayOneShot(clip, PlayerPrefs.GetFloat("Volume"));
            yield return new WaitWhile(() => kitchenAudioSource.isPlaying || IsGamePaused());
        }

        kitchenAudioSource.PlayOneShot(foodFinishedClip, PlayerPrefs.GetFloat("Volume"));
        yield return new WaitWhile(() => kitchenAudioSource.isPlaying || IsGamePaused());
    }

    private bool IsGamePaused() => Time.timeScale == 0;

    private AudioClip GenerateRandomKitchenSound(List<AudioClip> availableClips)
    {
        int index = Random.Range(0, availableClips.Count);
        return availableClips[index];
    }
}
