using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] Image hungerColumn;
    [SerializeField] Image healthColumn;

    [SerializeField] TextMeshProUGUI hungerText;
    [SerializeField] TextMeshProUGUI healthText;

    public int hunger = 50;
    public int health = 50;
    public void UpdatePlayerStats(float hungerChange, float healthChange)
    {
        hungerColumn.fillAmount += (hungerChange / 100f);
        healthColumn.fillAmount += (healthChange / 100f);

        UpdateStats();
    }
    public void NextRound()
    {
        int hungerNow = Mathf.RoundToInt(hungerColumn.fillAmount * 100);
        hungerNow -= 15;

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
}
