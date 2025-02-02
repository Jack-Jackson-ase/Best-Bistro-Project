using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] private Transform playerCamera; // Kamera-Transform
    [SerializeField] private Transform playerBody;   // Spieler-Transform
    public float sensitivity = 100f; // Maus-Sensitivität

    private float xRotation = 0f; // Vertikale Rotation (Pitch)
    private float yRotation = 0f; // Horizontale Rotation (Yaw)

    [SerializeField] private float yawLimit = 90f; // Maximaler Winkel für Yaw (links/rechts)

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        Look(context.ReadValue<Vector2>());
    }
    public void Look(Vector2 lookInput)
    {
        // Splitte Mausbewegung in X- und Y-Komponenten
        float mouseX = lookInput.x * sensitivity * Time.deltaTime; // Horizontale Mausbewegung
        float mouseY = lookInput.y * sensitivity * Time.deltaTime; // Vertikale Mausbewegung

        // **Vertikale Bewegung (Pitch)**: Rotiert die Kamera um die X-Achse
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f); // Begrenzung der vertikalen Rotation (oben/unten)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // **Horizontale Bewegung (Yaw)**: Rotiert den Spieler-Body um die Y-Achse, mit Begrenzung
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -yawLimit, yawLimit); // Begrenzung der horizontalen Rotation + die Start Position
        playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public void ResetLook()
    {
        playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
        playerBody.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}