using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] AudioSource playerAudioSource;
    [SerializeField] AudioClip coughSound;

    public void PlayerCough()
    {
        playerAudioSource.PlayOneShot(coughSound);
    }
}
