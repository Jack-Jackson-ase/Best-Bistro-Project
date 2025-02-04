using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject optionMenu;

    [SerializeField] private Animator mainCameraAnimator;
    [SerializeField] private GameObject vehiclePref;

    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private AudioSource soundManagerSource;

    bool gameStarted = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetOptionsState();

        StartCoroutine(SpawnVehicleEveryNowAndThen());
    }

    public void StartGame()
    {
        StartCoroutine(OpenDoorAndEnter());
        gameStarted = true;
        menu.SetActive(false);
    }

    public void ToggelOptionsMenu()
    {
        optionMenu.SetActive(!optionMenu.activeSelf);
    }

    public void ChangeOptions()
    {
        PlayerPrefs.SetFloat("Mouse Sensitivity Raw", sensitivitySlider.value);
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);

        PlayerPrefs.Save();

        ChangeAllVolumes();
    }

    void SetOptionsState()
    {
        var options = SaveSystem.GetOptionsStats();

        volumeSlider.value = options.volume;
        sensitivitySlider.value = options.sensitivityRaw;
    }

    public void DeleteSaveGame()
    {
        SaveSystem.DeleteSaveGame("Game State");
    }

    void ChangeAllVolumes()
    {
        float volume = volumeSlider.value;
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
            mainCameraAnimator.SetTrigger("Camera Animation");
            yield return new WaitForSeconds(2.22f);
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
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
