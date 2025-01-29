using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGenerator : MonoBehaviour
{
    [SerializeField] GameObject allPlates;

    [SerializeField] GameObject[] normalFoods;
    [SerializeField] GameObject[] specialFoods;
    [SerializeField] GameObject[] dessertFoods;

    [SerializeField] GameObject[] plates;

    private List<GameObject> chosenFoods = new List<GameObject>();

    private void OnEnable()
    {
        chosenFoods.Clear();
        allPlates.SetActive(true);
        StartCoroutine(SpawnNormalFoods());

        StartCoroutine(SpawnDessertFoods());
    }

    IEnumerator SpawnNormalFoods()
    {
        for (int i = 0; i < 3; i++)
        {
            GenerateFoodPrefab(i);
            yield return new WaitForSeconds(1);
        }
    }
    void GenerateFoodPrefab(int objectNumber)
    {
        GameObject[] foodArray = ChooseFoodRarity();
        int index =Random.Range(0, foodArray.Length);
        GameObject food= Instantiate(foodArray[index], plates[objectNumber].transform.position + Vector3.up * 0.0000001f, normalFoods[index].transform.rotation);
        food.transform.position += food.transform.up * 0.1f;

        chosenFoods.Add(food);
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