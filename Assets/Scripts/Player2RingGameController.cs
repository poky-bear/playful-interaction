using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2RingGameController : RingGameController
{
    // Override the Update method to use F key instead of spacebar
    void Update()
    {
        if (gameCompleted)
        {
            // When game is completed, make the cube follow the sphere
            if (cubeObject != null && concentricRings != null && concentricRings.targetSphere != null)
            {
                // Turn on controller script for cube object
                cubeObject.transform.position = concentricRings.targetSphere.transform.position;
            }
            return;
        }
            
        // Handle F key input instead of spacebar
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartExpanding();
        }
        
        if (Input.GetKeyUp(KeyCode.F))
        {
            CheckHit();
        }
        
        // Update expanding circle
        if (isExpanding)
        {
            // Increase the radius based on time
            currentRadius += expansionSpeed * Time.deltaTime;
            
            // Update the scale of the expanding circle
            expandingCircle.transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);
            
            // Ensure the expanding circle stays centered on the sphere
            expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
        }
        
        // Always keep the expanding circle centered on the sphere, even when not expanding
        // This ensures the distance calculation is always relative to the sphere's current position
        if (expandingCircle != null && expandingCircle.activeSelf)
        {
            expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
        }
    }
}