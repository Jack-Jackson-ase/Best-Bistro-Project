using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseSelectOtherStuff : MonoBehaviour
{
    Camera m_Camera;
    [SerializeField] StateMachine stateMachine;

    [SerializeField] BasicFood standartFoodScript; // KEIN "new BasicFood()", weil MonoBehaviour nicht mit "new" erstellt wird!
    [SerializeField] ChosenFoodUI selectedFoodUI;

    [SerializeField] Vector3 offsetForFoodCamera;

    BasicFood selectedFoodScript;
    bool foodSelected = false;

    private void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (foodSelected == false)
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Mouse.current.position.value);

            if (Physics.Raycast(ray, out hit))
            {
                MonoBehaviour[] scripts = hit.transform.gameObject.GetComponents<MonoBehaviour>();

                foreach (MonoBehaviour script in scripts)
                {
                    if (script is BasicFood foodScript) // Prüft, ob script von BasicFood erbt und castet es direkt
                    {
                        selectedFoodScript = foodScript;
                        break;
                    }
                }
            }
            if (Mouse.current.leftButton.IsPressed() && selectedFoodScript != null)
            {
                selectedFoodScript.IsSelected(true);
                foodSelected = true;

                StartCoroutine(MoveCameraToFoodAndActivateFoodUI());
            }
        }
        else if (Mouse.current.leftButton.IsPressed() && foodSelected)
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Mouse.current.position.value);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == selectedFoodScript.gameObject)
                {
                    stateMachine.FoodIsTaken(selectedFoodScript);
                }
            }
        }
    }

    public void DeselectFood()
    {
        selectedFoodScript.IsSelected(false);
        selectedFoodScript = null;
        foodSelected = false;
    }

    IEnumerator MoveCameraToFoodAndActivateFoodUI()
    {
        GetComponent<Animator>().SetInteger("Chosen Food", selectedFoodScript.foodNumber);
        yield return new WaitForSeconds(1f);

        selectedFoodUI.gameObject.SetActive(true);
        selectedFoodUI.ActivateUIForChosenFood(selectedFoodScript);
    }
}
