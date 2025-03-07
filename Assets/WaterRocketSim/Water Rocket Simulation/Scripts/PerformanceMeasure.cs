using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceMeasure : MonoBehaviour
{
    // Rocket performance is measured on duration of the flight and
    // the maximum height achieved. Egg being intact might be included
    // in a later version!
    public float flightTime = 0f;
    public float maxHeight = 0f;
    public float currentHeight = 0f;
    private bool timing = true;
    private float startHeight;
    private Transform body;
    
    

    void Start()
    {

        body = transform.Find("Body");
        
        
        // Use the starting height of the rocket as zero reference height
        startHeight = body.position.y;
        currentHeight = GetRelativeHeight();

        // Start timing the flight duration
        //StartTimer();
        
    }

    // When the rocket collides with something, stop timing - it's hit the ground. Not currently working
    private void OnCollisionEnter(Collision collision)
    {
        StopTimer();
    }

    // Gets the height of the rocket relative to the starting height
    float GetRelativeHeight()
    {
        return body.position.y - startHeight;
    }


    void FixedUpdate()
    {
        if (timing & Time.time>1)
        {
            // Add the time step onto our timer
            flightTime += Time.fixedDeltaTime;

            // Get current height of the rocket
            currentHeight = GetRelativeHeight();

            if (currentHeight > maxHeight)
            {
                // Update max height with current height if we exceeded previous max
                maxHeight = currentHeight;
            }
            // stop timer if below start height (must have hit the ground)
            if (currentHeight <-.1f) StopTimer();
        }
       

    }

    void StartTimer()
    {
        timing = true;
    }

    void StopTimer()
    {
        timing = false;
    }
}
