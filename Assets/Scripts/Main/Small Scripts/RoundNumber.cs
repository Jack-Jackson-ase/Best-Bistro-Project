using TMPro;
using UnityEngine;

public class RoundNumber : MonoBehaviour
{
    [SerializeField] StateMachine stateMachine;
    [SerializeField] string extraText;
    private void OnEnable()
    {
        ShowRoundNumber();
    }

    public void ShowRoundNumber()
    {
        GetComponent<TextMeshProUGUI>().text = extraText + stateMachine.roundNumber.ToString();
    }
}
