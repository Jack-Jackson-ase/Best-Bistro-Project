using System.Collections;
using UnityEngine;

public abstract class BasicFoodBehaviour : MonoBehaviour
{
    Rigidbody foodRb;
    [SerializeField] protected virtual StateMachine stateMachine { get; set; }
    [SerializeField] protected virtual Animator cameraAnimator { get; set; }
    [SerializeField] protected virtual PlayerBehaviour playerBehaviour { get; set; }

    [SerializeField] private int foodValueMin;
    [SerializeField] private int foodValueMax;

    public virtual int FoodValueMin
    {
        get => foodValueMin;
        set => foodValueMin = value;
    }

    public virtual int FoodValueMax
    {
        get => foodValueMax;
        set => foodValueMax = value;
    }

    bool isAlreadySelected = false;

    public int foodNumber;
    // Virtual properties
    public virtual float foodValue { get; set; }
    public virtual float healthValue { get; set; }


    public virtual void Start()
    {
        foodRb = GetComponent<Rigidbody>();

        cameraAnimator = GameObject.Find("Player Camera").GetComponent<Animator>();
        playerBehaviour = GameObject.Find("Player Camera").GetComponent<PlayerBehaviour>();
        stateMachine = GameObject.Find("State Machine").GetComponent<StateMachine>();

        GenerateProperties();
    }
    public virtual void GenerateProperties()
    {
        foodValue = Random.Range(foodValueMin, foodValueMax);
        healthValue = Mathf.Floor(Random.Range(foodValueMin, foodValueMax) / Random.Range(2f, 3f));
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

    public abstract void ActivateChosenFood(PlayerStats playerStatsScript);
}
