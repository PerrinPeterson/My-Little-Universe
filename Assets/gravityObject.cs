using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;


[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class gravityObject : MonoBehaviour
{
    public bool ApplyGravity = true;
    private float G = 0.0f;

    // Each of these variables should be able to be set and will effect the rest of the variables
    // Ie; setting any of the variables in the surface gravity equation, g = G*(M/r^2), will rearrange the equation to solve for another variable
    // Surface Gravity = g = G*(M/r^2), technically this should also take into account the mass of the object being affected by the gravity, but we'll ignore that for now
    public float Mass = 1.0f; 
    public float SurfaceGravity = 0.0f;
    public float Radius = 1.0f;
    public float GravitationalRadius;
    

    // These settings are for figuring out an ideal orbital radius, and velocity.
    // The idea is to also have these set up in a way where if you change one, another will be recalculated
    // Orbital Velocity = sqrt(G*M/r)
    public Vector3 StartingVelocity;
    public List<gravityObject> ObjectsInGravitationalRadius = new List<gravityObject>(); //List of objects within the gravitational radius of this object
    public int OrbitTime = 0; //Time it takes to orbit the central body
    public float IdealOrbitalRadius = 0.0f; //The ideal orbital radius of the object
    public float IdealOrbitVelocity = 0.0f;
    public gravityObject orbitingBody; //The object that this object wants to obit it.

    /*If I move an object, hitting the refresh button should refresh what's being affected by it's gravity*/
    public bool refresh = false;

    // These are fields that change with onValidate, so I can see what changed and recalculate the other values
    private float _mass;
    private float _surfaceGravity;
    private float _radius;
    private Vector3 _startingVelocity;
    private float _idealOrbitVelocity;
    private int _orbitTime;
    private float _idealOrbitalRadius;
    private gravityObject _orbitingBody;
    private float _G;



    // Start is called before the first frame update
    void Start()
    {
        //Setting the velocity of the object
        GetComponent<Rigidbody>().linearVelocity = StartingVelocity;

        // Calculating the gravitational radius of the object
        // g = G*(M/r^2) => r = sqrt(G*M/g) where g will be 0.01 m/s^2, aka too low to care to calculate
        // This is a small attempt at a future optimization, as I don't want to calculate the gravitational pull of objects that are so
        // far away that their effect is negligible.
        G = GameObject.Find("Universe").GetComponent<UniversalConstants>().GravitationalConstant; // Gravitational constant
        GravitationalRadius = Mathf.Sqrt(G * Mass / 0.000001f);


        _mass = Mass;
        _surfaceGravity = SurfaceGravity;
        _radius = Radius;
        _startingVelocity = StartingVelocity;
        _orbitTime = OrbitTime;
        _idealOrbitalRadius = IdealOrbitalRadius;
        _G = G;
        _idealOrbitVelocity = IdealOrbitVelocity;
    }

    // Changing the value of mass or velocity will change the value of the 
    private float CalculateIdealOrbitalRadius(float mass, float velocity)
    {
        // radius = cuberoot(2MGt^2Pi/2Pi) == GM/v^2, broke down v so I could specify a time
        float radius = Mathf.Pow((2 * Mass * G * Mathf.Pow(OrbitTime, 2) * Mathf.PI), 1f / 3f) / (2 * Mathf.PI);
        return radius;
    }

    // Update is called once per frame
    void Update()
    {
        if(refresh)
        {
            refresh = false; //Aesy way to call the validate function.
        }

        if (!Application.isPlaying)
        {
            return; //Don't do anything if the game isn't running
        }
        //ObjectsInGravitationalRadius.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, GravitationalRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != gameObject)
            {
                gravityObject obj = hitCollider.gameObject.GetComponent<gravityObject>();
                if (obj != null)
                {
                    if (!obj.ObjectsInGravitationalRadius.Contains(this))
                    {
                        obj.ObjectsInGravitationalRadius.Add(this);
                    }
                }
            }
        }
    }

    //Updates the object whenever the editor is updated.
    public void OnValidate()
    {
        //Update the objects
        GetComponent<Rigidbody>().mass = Mass;
        Transform transform = GetComponent<Transform>();
        transform.localScale = new Vector3(Radius, Radius, Radius);

        //Update the gravitational constant, 
        G = GameObject.Find("Universe").GetComponent<UniversalConstants>().GravitationalConstant;
        GravitationalRadius = Mathf.Sqrt(G * Mass / 0.000001f);

        // Gravity Calculations
        // If the radius, or the mass of the object, or the Gravitational Constant has changed, recalculate the surface gravity
        if (_mass != Mass || _radius != Radius || _G != G)
        {
            // Surface Gravity = g = GM/R^2, technically this should also take into account the mass of the object being affected by the gravity, but we'll ignore that for now
            SurfaceGravity = G * Mass / (Radius * Radius);
        }
        // If the Surface Gravity has changed, recalculate the mass
        else if (_surfaceGravity != SurfaceGravity)
        {
            // Mass = M = gr^2/m
            Mass = (SurfaceGravity * (Radius * Radius)) / G;
        }
        // g = G*(M/r^2) => r = sqrt(G*M/g) where g will be 0.001 m/s^2, aka too low to care to calculate
        GravitationalRadius = Mathf.Sqrt((G * Mass) / 0.001f); // This may be incorrect, will need to see

        // Orbital Calculations
        // If the time, mass, or G changes, recalculate the ideal orbital radius
        if (_mass != Mass || _orbitTime != OrbitTime || _G != G)
        {
            // radius = cuberoot(2MGt^2Pi/2Pi) == GM/v^2, broke down v so I could specify a time
            IdealOrbitalRadius = CalculateIdealOrbitalRadius(Mass, IdealOrbitVelocity);
        }

        // Calculate the ideal orbital Velocity if the radius changes
        // Not ideal for the moons, as moons are more easily effected by the star and the surrounding planets. This is mostly due to factors like the scale of the universe being smaller.
        if (_idealOrbitalRadius != IdealOrbitalRadius || orbitingBody != _orbitingBody)
        {
            // v = sqrt(GM/r)
            if (orbitingBody != null)
            {
                IdealOrbitVelocity = Mathf.Sqrt((G * (orbitingBody.Mass * Mass)) / IdealOrbitalRadius);
            }
            else
            {
                IdealOrbitVelocity = Mathf.Sqrt((G * Mass) / IdealOrbitalRadius);
            }
        }



        //get the objects around self that are within the gravitational radius, and add itself to their lists
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, GravitationalRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != gameObject)
            {
                gravityObject obj = hitCollider.gameObject.GetComponent<gravityObject>();
                if (obj != null)
                {
                    if (!obj.ObjectsInGravitationalRadius.Contains(this))
                    {
                        obj.ObjectsInGravitationalRadius.Add(this);
                    }
                }
            }
        }

        // Refresh the object's values
        _mass = Mass;
        _surfaceGravity = SurfaceGravity;
        _radius = Radius;
        _startingVelocity = StartingVelocity;
        _orbitTime = OrbitTime;
        _idealOrbitalRadius = IdealOrbitalRadius;
        _idealOrbitVelocity = IdealOrbitVelocity;
        _G = G;
        _orbitingBody = orbitingBody;
    }

    private void FixedUpdate()
    {
        if (!Application.isPlaying)
        {
            return; //Don't do anything if the game isn't running
        }
        //Apply gravity to the object
        if (ApplyGravity)
        {
            Vector3 acceleration = Vector3.zero;
            foreach (var obj in ObjectsInGravitationalRadius)
            {
                //

                Vector3 direction = obj.transform.position - transform.position;
                direction.Normalize();
                float distance = (obj.transform.position - transform.position).magnitude;
                float force = G * ((obj.Mass * Mass) / (distance * distance));
                float forceMultiplier = 1; //Trying to get my solar system to work properly, not sure if I need this actually
                force *= forceMultiplier;
                acceleration += direction * force / Mass;
            }
            GetComponent<Rigidbody>().linearVelocity += acceleration * Time.deltaTime;
        }
    }
}
