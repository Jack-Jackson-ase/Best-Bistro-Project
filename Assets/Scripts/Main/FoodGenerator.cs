using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGenerator : MonoBehaviour
{
    [SerializeField] GameObject allPlates;

    [SerializeField] GameObject[] normalFoods;
    [SerializeField] GameObject[] specialFoods;
    [SerializeField] GameObject[] dessertFoods;

    [SerializeField] GameObject platePrefab;
    [SerializeField] GameObject[] plateSpawnPositionsAsEmptyObjects;

    private List<GameObject> chosenFoods = new List<GameObject>();

    private void OnEnable()
    {
        chosenFoods.Clear();
        StartCoroutine(SpawnNormalFoods());

        StartCoroutine(SpawnDessertFoods());
    }

    IEnumerator SpawnNormalFoods()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject plateParent = new GameObject();
            plateParent.transform.position = plateSpawnPositionsAsEmptyObjects[i].transform.position;

            GameObject plate = Instantiate(platePrefab);
            plate.transform.parent = plateParent.transform;
            plate.transform.localPosition = Vector3.zero;

            GameObject food = GenerateFoodPrefab(i);
            food.transform.parent = plate.transform;
            food.transform.localPosition = Vector3.up * 0.01f;
            food.transform.rotation = Quaternion.Euler(0, Random.Range(0,360), 0);
            chosenFoods.Add(food);
        }
        yield return new WaitForSeconds(1);
    }
    GameObject GenerateFoodPrefab(int objectNumber)
    {
        GameObject[] foodArray = ChooseFoodRarity();
        int index = Random.Range(0, foodArray.Length);
        GameObject food = Instantiate(foodArray[index]);
        food.GetComponent<BasicFood>().foodNumber = objectNumber+1;
        return food;
    }

    GameObject[] ChooseFoodRarity()
    {
        int i = Random.Range(0, 10);
        if (i < 9)
        {
            return normalFoods;
        }
        else { return specialFoods; }
    }

    public void DestroyAllFoodsBut(BasicFood survivingFood)
    {
        foreach (GameObject food in chosenFoods)
        {
            if (food != survivingFood.gameObject)
            {
                Destroy(food.transform.parent.gameObject.transform.parent.gameObject);
            }
        }
    }
    IEnumerator SpawnDessertFoods()
    {
        {
            for (int i = 0; i < 2; i++)
            {
                GenerateDessertFood(i);
                yield return new WaitForSeconds(1);
            }
        }
    }

    void GenerateDessertFood(int index)
    {

    }
}