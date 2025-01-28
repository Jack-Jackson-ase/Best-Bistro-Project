using UnityEngine;

public class Car : MonoBehaviour
{
    float speed;
    public AudioSource audioSourceHonk;
    public AudioSource audioSourceLooping;

    [SerializeField] Color[] randomColorOfCar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSourceHonk.volume = PlayerPrefs.GetFloat("Volume");
        audioSourceHonk.pitch = Random.Range(0f, 2f);
        audioSourceLooping.volume = PlayerPrefs.GetFloat("Volume");
        speed = Random.Range(2.5f, 6f);
        RandomizeColor();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        CheckIfOutOfBounds();
    }

    private void Move()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void CheckIfOutOfBounds()
    {
        if (transform.position.x < -6)
        {
            Destroy(gameObject);
        } ;
    }

    void RandomizeColor()
    {
        int index = Random.Range(0, randomColorOfCar.Length);
        Material material = transform.Find("Body").GetComponent<Renderer>().material;
        material.color = randomColorOfCar[index];
    }
}
