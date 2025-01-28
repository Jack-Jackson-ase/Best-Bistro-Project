using UnityEngine;

public class CameraInsideTrigger : MonoBehaviour
{
    [SerializeField] GameObject blackOverlay;

    void OnTriggerEnter(Collider other)
    {
            blackOverlay.SetActive(true); //Show black overlay
    }

    void OnTriggerExit(Collider other)
    {
            blackOverlay.SetActive(false); // Hide black overlay
    }
}