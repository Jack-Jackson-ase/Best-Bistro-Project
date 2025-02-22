using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] Image hungerColumn;
    [SerializeField] Image healthColumn;

    [SerializeField] TextMeshProUGUI hungerText;
    [SerializeField] TextMeshProUGUI healthText;

    [SerializeField] TextMeshProUGUI hungerAddingText;
    [SerializeField] TextMeshProUGUI healthAddingText;

    public int hunger = 50;
    public int health = 50;
    public void UpdatePlayerStats(float hungerChange, float healthChange)
    {
        hungerColumn.fillAmount += (hungerChange / 100f);
        healthColumn.fillAmount += (healthChange / 100f);

        StartCoroutine(ShowNumberThatChanges(hungerAddingText, hungerChange));
        StartCoroutine(ShowNumberThatChanges(healthAddingText, healthChange));

        UpdateStats();
    }
    public void NextRound(int roundNumber)
    {
        int hungerNow = Mathf.RoundToInt(hungerColumn.fillAmount * 100);
        hungerNow -= 20 + Mathf.Clamp(Mathf.FloorToInt(roundNumber / 5), 0, 5) + Mathf.Clamp(Mathf.FloorToInt((roundNumber - 25) / 10), 0, 5);

        if (hungerNow < 0)
        {
            hungerColumn.fillAmount = 0;
            healthColumn.fillAmount += hungerNow / 100f;
        }
        else
        {
            hungerColumn.fillAmount = hungerNow / 100f;
        }

        UpdateStats();
    }
    public void SetPlayerStats(int hunger, int health)
    {
        hungerColumn.fillAmount = hunger / 100f;
        healthColumn.fillAmount = health / 100f;

        UpdateStats();
    }

    void UpdateStats()
    {
        hunger = Mathf.RoundToInt(hungerColumn.fillAmount * 100);
        health = Mathf.RoundToInt(healthColumn.fillAmount * 100);

        hungerText.text = hunger.ToString();
        healthText.text = health.ToString();
    }

    IEnumerator ShowNumberThatChanges(TextMeshProUGUI changingNumberText, float number)
    {
        if (number != 0)
        {
            if (number > 0)
            {
                changingNumberText.color = Color.green;
                changingNumberText.text = "+" + Mathf.Round(number).ToString();
            }
            else if (number < 0)
            {
                changingNumberText.color = Color.red;
                changingNumberText.text = Mathf.Round(number).ToString();
            }

            yield return new WaitForSeconds(0.3f);

            while (changingNumberText.color.a > 0.0f)
            {
                changingNumberText.color = new Color(changingNumberText.color.r, changingNumberText.color.g, changingNumberText.color.b, changingNumberText.color.a - (Time.deltaTime / 1.5f));
                yield return null;
            }
        }
    }
}
