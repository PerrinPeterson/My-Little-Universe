using UnityEngine;

public class FollowPlanet : MonoBehaviour
{

    public GameObject planet;
    private Vector3 offset;
    public Vector3 rotationSpeed = new Vector3(0, 10, 0); // Speed of rotation around the planet
    private Vector3 angle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Calculate the offset between the camera and the planet
        offset = transform.position - planet.transform.position;

        // Set the initial angle to the planet's rotation
        angle = planet.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate around the planet
        angle += rotationSpeed * Time.deltaTime; // Update the angle based on the rotation speed
        // Calculate the new position of the camera
        Quaternion rotation = Quaternion.Euler(angle);
        Vector3 newPosition = planet.transform.position + rotation * offset;
        // Update the camera's position
        transform.position = newPosition;
        // Make the camera look at the planet
        transform.LookAt(planet.transform.position);

    }
}
