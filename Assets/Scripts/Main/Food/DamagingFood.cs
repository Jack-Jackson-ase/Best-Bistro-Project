using System.Collections;
using UnityEngine;

public class DamagingFood : BasicFoodBehaviour
{
    public override void GenerateProperties()
    {
        foodValue = Random.Range(FoodValueMin, FoodValueMax) + 10;
        healthValue = Mathf.Floor(Random.Range(-FoodValueMin / Random.Range(2f, 3.5f), -FoodValueMax / Random.Range(1.5f, 3.5f)));
    }

    public override void ActivateChosenFood(PlayerStats playerStatsScript)
    {
        Debug.Log("Food Script is activated");
        playerStatsScript.UpdatePlayerStats(foodValue, healthValue);
        StartCoroutine(FoodEffectActivates());
    }

    IEnumerator FoodEffectActivates()
    {
        Debug.Log("Food Script IEnumerator is started");
        cameraAnimator.SetTrigger("Cough");
        yield return new WaitForSeconds(4.3f);
        stateMachine.SpitBlood();
        playerBehaviour.PlayerCough();
        yield return new WaitForSeconds(1f);
        stateMachine.FoodHasFinishedTakingEffect();
    }
}
