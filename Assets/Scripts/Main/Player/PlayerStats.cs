using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] Image hungerColumn;
    [SerializeField] Image healthColumn;

    [SerializeField] TextMeshProUGUI hungerText;
    [SerializeField] TextMeshProUGUI healthText;
    public void UpdatePlayerStats(float hungerChange, float healthChange)
    {
        hungerColumn.fillAmount += (hungerChange/100f);
        healthColumn.fillAmount += (healthChange/100f);

        hungerText.text = hungerColumn.fillAmount.ToString();
        healthText.text = healthColumn.fillAmount.ToString();
    }
    public void NextRound()
    {
        if (hungerColumn.fillAmount != 0)
        {
            hungerColumn.fillAmount -= 0.15f;
            hungerText.text = (hungerColumn.fillAmount).ToString();
        }
        else
        {
            healthColumn.fillAmount -= 15f;
            healthText.text = (healthColumn.fillAmount *100).ToString() ;
        }
    }
}
