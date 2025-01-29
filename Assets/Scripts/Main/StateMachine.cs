using System;
using System.IO;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [SerializeField] MouseLook mouseLook;
    [SerializeField] PauseSystem pauseSystem;
    [SerializeField] FoodGenerator generateFoodScript;

    private enum playerInteraction
    {
        Sequence,
        Playable,
    }
    private playerInteraction playerState;

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

    public void ChangeGameState(gameState state)
    {
        switch (state)
        {
            case gameState.FoodSelection:
                TogglePlayerInput(true);
                generateFoodScript.enabled = true;
                break;

            case gameState.FoodChosen:
                TogglePlayerInput(false);
                generateFoodScript.enabled = false;
                //Delete Other Foods
                //Activate Eating Chosen Food
                break;
            case gameState.FoodTakesEffect:
                SaveState(gameState.FoodTakesEffect);
                TogglePlayerInput(true);
                generateFoodScript.enabled = false;
                //Activate Effects
                break;
        }
        void SaveState(gameState state)
        {
            string stateToString = state.ToString();
            SaveSystem.SaveData(stateToString, "Game State");
        }

        void TogglePlayerInput(bool toggleOnOrOff)
        {
            mouseLook.enabled = toggleOnOrOff;
            pauseSystem.enabled = toggleOnOrOff;
            if (toggleOnOrOff)
            {
                playerState = playerInteraction.Playable;
            }
            else
            {
                playerState = playerInteraction.Sequence;
            }
        }
    }
}
