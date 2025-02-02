using UnityEngine;
using UnityEngine.UI;

public class ChosenFoodUI : MonoBehaviour
{
    [SerializeField] Image smellColumn;
    [SerializeField] Image designColumn;

    [SerializeField] Animator cameraAnimator;
    [SerializeField] MouseSelectOtherStuff selectScript;

    public void ActivateUIForChosenFood(BasicFood foodScript)
    {
        smellColumn.fillAmount = foodScript.smellNumberNormalized;
        designColumn.fillAmount = foodScript.designNumberNormalized;
    }

    public void DeactivateChosenFood()
    {
        cameraAnimator.SetInteger("Chosen Food", 0);
        selectScript.DeselectFood();
        gameObject.SetActive(false);
    }
}