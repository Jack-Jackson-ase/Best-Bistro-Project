using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChosenFoodUI : MonoBehaviour
{
    [SerializeField] Image smellColumn;
    [SerializeField] Image designColumn;

    [SerializeField] StateMachine stateMachine;
    [SerializeField] Animator cameraAnimator;
    [SerializeField] MouseSelectOtherStuff selectScript;

    public void ActivateUIForChosenFood(BasicFoodBehaviour foodScript)
    {
        smellColumn.fillAmount = foodScript.foodValue / 30f;
        designColumn.fillAmount = foodScript.healthValue / 30f;

        StartCoroutine(SlideBetweenColumnEdges(smellColumn, Mathf.Clamp(smellColumn.fillAmount + Random.Range(0f, 0.2f), 0, 1), Mathf.Clamp(smellColumn.fillAmount - Random.Range(0f, 0.2f), 0, 1)));
        StartCoroutine(SlideBetweenColumnEdges(designColumn, Mathf.Clamp(designColumn.fillAmount + Random.Range(0f, 0.2f), 0, 1), Mathf.Clamp(designColumn.fillAmount - Random.Range(0f, 0.2f), 0, 1)));
    }

    public void DeactivateChosenFood()
    {
        stateMachine.DeselectFood();
        gameObject.SetActive(false);
    }

    IEnumerator SlideBetweenColumnEdges(Image column, float max, float min)
    {
        bool addValue = false;
        while (gameObject.activeSelf)
        {
            if (!addValue)
            {
                if (column.fillAmount <= min) { addValue = true; };
                column.fillAmount -= 0.3f * Time.deltaTime;
            }
            else if (addValue)
            {
                if (column.fillAmount >= max) { addValue = false; }
                column.fillAmount += 0.3f * Time.deltaTime;
            }
            yield return null;
        }
    }
}