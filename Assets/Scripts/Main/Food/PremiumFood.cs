using System.Collections;
using UnityEngine;
using static SpecialBonusManager;

public class PremiumFood : BasicFoodBehaviour
{
    private SpecialBonusManager.SpecialEffects SpecialEffects;
    public override void ActivateChosenFood(PlayerStats playerStatsScript)
    {
        playerStatsScript.UpdatePlayerStats(foodValue, healthValue);
        StartCoroutine(FoodEffectActivates());
    }

    IEnumerator FoodEffectActivates()
    {
        SpecialBonusManager.Instance.ActivateEffect(GenerateEffect());
        yield return new WaitForSeconds(3f);
        stateMachine.FoodHasFinishedTakingEffect();
    }

    SpecialEffects GenerateEffect()
    {
        float index = Random.value;

        if (index < 0.2)
        {
            return SpecialEffects.SharperSenses;
        }
        else if (index < 0.4)
        {
            return SpecialEffects.SharperSenses;
        }
        else if (index < 0.6)
        {
            return SpecialEffects.SharperSenses;
        }
        else if (index < 0.8)
        {
            return SpecialEffects.SharperSenses;
        }
        else
        {
            return SpecialEffects.SharperSenses;
        }
    }
}
