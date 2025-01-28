using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject optionMenu;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject vehiclePref;
    [SerializeField] private GameObject enterHouseBlock;

    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioSource soundManagerSource;

    bool gameStarted = false;
    float volume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.HasKey("Value") != true)
        {
            PlayerPrefs.SetFloat("Value", 0.5f);
            Debug.Log("Player Prefs Value was zero, now 0.5f");
        }
        volume = PlayerPrefs.GetFloat("Value");
        volumeSlider.value = volume;
        soundManagerSource.volume = volume;

        StartCoroutine(SpawnVehicleEveryNowAndThen());
    }

    public void StartGame()
    {
        StartCoroutine(OpenDoorAndEnter());
        gameStarted = true;
        menu.SetActive(false);
        DataManager.instance.volume = volume;
    }

    public void ToggelOptionsMenu()
    {
        optionMenu.SetActive(!optionMenu.activeSelf);
    }

    public void ChangeVolume()
    {
        volume = volumeSlider.value;

        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();

        ChangeAllVolumes();
    }
    void ChangeAllVolumes()
    {
        soundManagerSource.volume = volume;

        var cars = FindObjectsByType<Car>(FindObjectsSortMode.None);
        foreach (Car car in cars)
        {
            car.audioSourceHonk.volume = volume;
            car.audioSourceLooping.volume = volume;
        }
    }

    public void ExitGame()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
    IEnumerator OpenDoorAndEnter()
    {
        if (gameStarted == false)
        {
            StartCoroutine(DoorOpening());
            StartCoroutine(CameraMovingIn());
            yield return new WaitForSeconds(2.22f);
            SceneManager.LoadScene(1);
        }
    }
    IEnumerator DoorOpening()
    {
        while (true)
        {
            doorLeft.transform.Rotate(0, 50 * Time.deltaTime, 0);
            doorRight.transform.Rotate(0, -50 * Time.deltaTime, 0);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator CameraMovingIn()
    {
        while (true)
        {
            mainCamera.transform.position += Vector3.forward * 0.7f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator SpawnVehicleEveryNowAndThen()
    {
        float spawnRateVehicle = Random.Range(5, 10);
        yield return new WaitForSeconds(spawnRateVehicle);
        SpawnCar();
        StartCoroutine(SpawnVehicleEveryNowAndThen());
    }

    void SpawnCar()
    {
        Instantiate(vehiclePref, new Vector3(9, -0.2f, -4.1f), vehiclePref.transform.rotation);
    }
}
