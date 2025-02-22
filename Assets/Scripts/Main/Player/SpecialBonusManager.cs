using System.Collections;
using TMPro;
using UnityEngine;

public class SpecialBonusManager : MonoBehaviour
{
    public static SpecialBonusManager Instance { get; private set; }

    [SerializeField] TextMeshProUGUI dialougeText;
    [SerializeField] ChosenFoodUI foodUIScript;

    public enum SpecialEffects
    {
        SharperSenses,
        LessSharpSenses
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ActivateEffect(SpecialEffects effect)
    {
        switch (effect)
        {
            case SpecialEffects.SharperSenses:
                foodUIScript.ColumnsRangeNormalized -= 0.05f;
                StartCoroutine(ShowDescriptonText("It feels like my senses are sharper now!", 3f));
                break;

            case SpecialEffects.LessSharpSenses:
                foodUIScript.ColumnsRangeNormalized += 0.05f;
                StartCoroutine(ShowDescriptonText("My senses feel numb", 3f));
                break;
        }
    }

    IEnumerator ShowDescriptonText(string description, float time)
    {
        yield return new WaitForSeconds(0.5f);
        dialougeText.text = description;
        yield return new WaitForSeconds(time);
        dialougeText.text = null;
    }
}