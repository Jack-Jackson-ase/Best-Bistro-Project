using UnityEngine;
using UnityEngine.InputSystem;

public class MouseSelectOtherStuff : MonoBehaviour
{
    private Camera m_Camera;
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private PlayerInput input;
    [SerializeField] private ChosenFoodUI selectedFoodUI;

    private BasicFoodBehaviour selectedFoodScript;
    private bool foodIsSelected = false;
    public bool isActive = false;

    private void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!isActive) return;

        CheckForFoodSelection();
    }

    public void Activate()
    {
        isActive = true;
        ClearSelectedFood();
    }
    public void ClearSelectedFood()
    {
        foodIsSelected = false;
        selectedFoodScript = null;
    }

    public void Deactivate()
    {
        isActive = false;
        ClearSelectedFood();
    }

    private void CheckForFoodSelection()
    {
        if (!input.actions.FindAction("Look").enabled && Cursor.visible)
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Mouse.current.position.value);

            if (Physics.Raycast(ray, out hit))
            {
                selectedFoodScript = hit.transform.GetComponent<BasicFoodBehaviour>();
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && selectedFoodScript != null)
            {
                if (stateMachine.selectedFood == false)
                {
                    foodIsSelected = true;
                    stateMachine.OneFoodIsSelected(selectedFoodScript);
                }
                else if (selectedFoodScript == stateMachine.selectedFood)
                {
                    foodIsSelected = false;
                    stateMachine.FoodIsTaken();
                }
            }
        }
    }
}
