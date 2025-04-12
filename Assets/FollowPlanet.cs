using UnityEngine;

public class FollowPlanet : MonoBehaviour
{

    public GameObject planet;
    private Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Calculate the offset between the camera and the planet
        offset = transform.position - planet.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the camera to the planet's position plus the offset
        transform.position = planet.transform.position + offset;
    }
}
