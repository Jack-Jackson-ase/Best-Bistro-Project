using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public float volume;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
    }
}
