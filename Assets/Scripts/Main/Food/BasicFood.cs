using System.Collections;
using UnityEngine;

public class BasicFood : MonoBehaviour
{
    Rigidbody foodRb;

    [SerializeField] int foodValueMin;
    [SerializeField] int foodValueMax;

    bool isAlreadySelected = false;

    public int foodNumber;
    public int foodValue;
    public int healthValue;

    public float smellNumberNormalized;
    public float designNumberNormalized;

    public virtual void Start()
    {
        foodRb = GetComponent<Rigidbody>();
        foodValue = Random.Range(foodValueMin, foodValueMax);
        healthValue = Random.Range(foodValueMin, foodValueMax);

        if (Random.value < 0.5f)
        {
            smellNumberNormalized = GenerateProperties(foodValue);
            designNumberNormalized = GenerateProperties(healthValue);
        }
        else
        {
            smellNumberNormalized = GenerateProperties(healthValue);
            designNumberNormalized = GenerateProperties(foodValue);
        }
    }

    private float GenerateProperties(float bases)
    {
        float smell = Random.Range(foodValue - 8, foodValue + 8);
        float smellNormalized = Mathf.Clamp(smell, 0, 20) / 20;
        return smellNormalized;
    }

    public virtual void IsSelected(bool isSelected)
    {
        if (isSelected && !isAlreadySelected)
        {
            isAlreadySelected = true;
            foodRb.isKinematic = true;

            StartCoroutine(TurnFood());
            transform.localPosition = Vector3.up * 0.1f;
        }
        else if (!isSelected && isAlreadySelected)
        {
            foodRb.isKinematic = false;
            isAlreadySelected = false;

            StopCoroutine(TurnFood());
        }
    }

    public virtual IEnumerator TurnFood()
    {
        var waitForEndOFFrame = new WaitForEndOfFrame();
        while (isAlreadySelected)
        {
            transform.Rotate(Vector3.up * 30 * Time.deltaTime);
            yield return waitForEndOFFrame;
        }
    }

    public virtual void ActivateChosenFood(PlayerStats playerStatsScript)
    {
        playerStatsScript.UpdatePlayerStats(foodValue, healthValue);
    }
}
