using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] Image hungerColumn;
    [SerializeField] Image healthColumn;

    [SerializeField] Animator cameraAnimator;
    [SerializeField] MouseSelectOtherStuff selectScript;

    public void UpdatePlayerStats(int hungerChange, int healthChange)
    {
        hungerColumn.fillAmount += hungerChange;
        healthColumn.fillAmount += healthChange;
    }

}
