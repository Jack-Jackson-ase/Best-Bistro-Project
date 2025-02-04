using System.Collections;
using UnityEngine;

public class PremiumFood : BasicFoodBehaviour
{
    public override void ActivateChosenFood(PlayerStats playerStatsScript)
    {
        playerStatsScript.UpdatePlayerStats(foodValue, healthValue);
        StartCoroutine(FoodEffectActivates());
    }

    IEnumerator FoodEffectActivates()
    {
        yield return new WaitForSeconds(3f);
        stateMachine.FoodHasFinishedTakingEffect();
    }
}
