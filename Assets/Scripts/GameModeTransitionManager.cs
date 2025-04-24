using UnityEngine;
using System.Collections;

public class GameModeTransitionManager : MonoBehaviour
{
    [Header("Boid Settings")]
    public BoidSettings boidSettings;
    public float speedIncreaseRate = 2f; // Units per second
    public float maxSpeedIncrease = 10f; // Maximum additional speed

    [Header("Cone Settings")]
    public GameObject conePrefab;
    private Vector3[] cornerPositions;
    private float cornerOffset = 10f; // Distance from center to spawn corners

    private BoidManager boidManager;
    private float originalMinSpeed;
    private float originalMaxSpeed;
    private bool isTransitioning = false;

    void Start()
    {
        boidManager = FindObjectOfType<BoidManager>();
        if (boidSettings == null && boidManager != null)
        {
            boidSettings = boidManager.settings;
        }

        if (boidSettings != null)
        {
            originalMinSpeed = boidSettings.minSpeed;
            originalMaxSpeed = boidSettings.maxSpeed;
        }

        // Calculate corner positions
        cornerPositions = new Vector3[4]
        {
            new Vector3(cornerOffset, 0, cornerOffset),   // Front Right
            new Vector3(cornerOffset, 0, -cornerOffset),  // Back Right
            new Vector3(-cornerOffset, 0, cornerOffset),  // Front Left
            new Vector3(-cornerOffset, 0, -cornerOffset)  // Back Left
        };
    }

    public void StartTransition()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionSequence());
        }
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // Wait for 10 seconds
        yield return new WaitForSeconds(10f);

        // Gradually increase boid speed over 5 seconds
        float elapsedTime = 0f;
        float initialMinSpeed = boidSettings.minSpeed;
        float initialMaxSpeed = boidSettings.maxSpeed;

        while (elapsedTime < 5f)
        {
            float speedIncrease = (elapsedTime / 5f) * maxSpeedIncrease;
            boidSettings.minSpeed = initialMinSpeed + speedIncrease;
            boidSettings.maxSpeed = initialMaxSpeed + speedIncrease;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset speeds to original values
        boidSettings.minSpeed = originalMinSpeed;
        boidSettings.maxSpeed = originalMaxSpeed;

        // Spawn cone in random corner
        SpawnCone();

        isTransitioning = false;
    }

    private void SpawnCone()
    {
        if (conePrefab != null)
        {
            // Pick a random corner
            int randomCorner = Random.Range(0, 4);
            Vector3 spawnPosition = cornerPositions[randomCorner];

            // Instantiate the cone
            GameObject cone = Instantiate(conePrefab, spawnPosition, Quaternion.identity);
            
            // Point the cone towards the center
            cone.transform.LookAt(Vector3.zero);
            // Adjust rotation to point upward
            cone.transform.Rotate(90f, 0f, 0f);
        }
        else
        {
            Debug.LogError("Cone prefab not assigned to GameModeTransitionManager!");
        }
    }
}