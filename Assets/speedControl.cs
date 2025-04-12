using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedControl : MonoBehaviour
{
    private float currentSpeed = 1.0f;
    public float speed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (speed != currentSpeed)
        {
            if (speed < 0.000001)
            {
                speed = 0.000001f;
            }
            if (speed > 100)
                speed = 100.0f;
            currentSpeed = speed;
            Time.timeScale = speed;
        }
        
    }
}
