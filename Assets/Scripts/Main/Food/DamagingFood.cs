using System.Collections;
using UnityEngine;

public class DamagingFood : BasicFoodBehaviour
{
    //[SerializeField] ParticleSystem coughParticles;
    public override void GenerateProperties()
    {
        foodValue = Random.Range(FoodValueMin, FoodValueMax);
        healthValue = Random.Range(-FoodValueMin, -FoodValueMax);
    }

    public override void ActivateChosenFood(PlayerStats playerStatsScript)
    {
        playerStatsScript.UpdatePlayerStats(foodValue, healthValue);
        StartCoroutine(FoodEffectActivates());
    }

    IEnumerator FoodEffectActivates()
    {
        cameraAnimator.SetTrigger("Cough");
        yield return new WaitForSeconds(0.2666f);
        //coughParticles.Play();
        yield return new WaitForSeconds(0.8333f);
        //coughParticles.Play();
        yield return new WaitForSeconds(2.3999f);
        stateMachine.FoodHasFinishedTakingEffect();
    }
}
