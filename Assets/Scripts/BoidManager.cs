using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    public Transform target;  // Reference to the target object (legacy)
    public Transform player1Target; // Reference to player 1
    public Transform player2Target; // Reference to player 2
    private bool isMultiplayerMode = false;
    private bool isGameWon = false; // Flag to track if the game has been won
    public float splitDistance = 5f; // Distance at which the flock splits
    Boid[] boids;

    // Colors for multiplayer mode
    public Color unifiedFlockColor = new Color(0.8f, 0.4f, 0.8f, 1f); // Purple for unified flock
    public Color player1Color = new Color(1f, 0.4f, 0.4f, 1f); // Red for player 1
    public Color player2Color = new Color(0.4f, 0.4f, 1f, 1f); // Blue for player 2

    public void SetMultiplayerMode(bool active, Transform p1Target = null, Transform p2Target = null)
    {
        isMultiplayerMode = active;
        if (active)
        {
            player1Target = p1Target;
            player2Target = p2Target;
            Debug.Log("BoidManager: Multiplayer mode activated with " + boids.Length + " boids");

            // Set player colors
            if (player1Target != null)
            {
                Renderer p1Renderer = player1Target.GetComponent<Renderer>();
                if (p1Renderer != null) p1Renderer.material.color = player1Color;
            }
            if (player2Target != null)
            {
                Renderer p2Renderer = player2Target.GetComponent<Renderer>();
                if (p2Renderer != null) p2Renderer.material.color = player2Color;
            }
        }
        else
        {
            // In single player mode, use the legacy target
            player1Target = target;
            player2Target = null;
            Debug.Log("BoidManager: Returning to single player mode");

            // Reset player colors if possible
            if (p1Target != null)
            {
                Renderer p1Renderer = p1Target.GetComponent<Renderer>();
                if (p1Renderer != null) p1Renderer.material.color = Color.white;
            }
            if (p2Target != null)
            {
                Renderer p2Renderer = p2Target.GetComponent<Renderer>();
                if (p2Renderer != null) p2Renderer.material.color = Color.white;
            }
        }

        // Update boid targets and colors
        if (boids != null)
        {
            foreach (Boid b in boids)
            {
                if (b != null)
                {
                    if (active)
                    {
                        // In multiplayer mode, randomly assign boids to players
                        b.playerAssignment = (Random.value < 0.5f) ? 1 : 2;
                        // Initial color will be unified since players start close together
                        b.SetColour(unifiedFlockColor);
                    }
                    else
                    {
                        // In single player mode, all boids follow the main target
                        b.playerAssignment = 0;
                        b.SetColour(Color.white);
                    }
                }
            }
        }
    }
    
    // Call this method when the multiplayer game is won
    public void SetGameWon(bool won)
    {
        isGameWon = won;
        Debug.Log("BoidManager: Game won state set to " + won);
        
        // When the game is won, update all boids to use the unified flock color
        if (won && boids != null)
        {
            foreach (Boid b in boids)
            {
                if (b != null)
                {
                    b.SetColour(unifiedFlockColor);
                }
            }
        }
    }

    void Start () {
        // Find and initialize all boids
        boids = FindObjectsOfType<Boid> ();
        foreach (Boid b in boids) {
            if (b != null) {
                b.Initialize (settings, target);
            }
        }
    }

    private Vector3 GetFlockCenter() {
        if (!isMultiplayerMode || player1Target == null || player2Target == null) {
            return target != null ? target.position : Vector3.zero;
        }

        float distance = Vector3.Distance(player1Target.position, player2Target.position);
        if (distance <= splitDistance) {
            // Players are close, use midpoint
            return (player1Target.position + player2Target.position) / 2f;
        }
        
        // Players are far apart, boids should follow their assigned player
        return Vector3.zero; // This is a placeholder, actual targeting is done per boid
    }

    void Update () {
        if (boids != null) {

            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++) {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer (numBoids, BoidData.Size);
            boidBuffer.SetData (boidData);

            compute.SetBuffer (0, "boids", boidBuffer);
            compute.SetInt ("numBoids", boids.Length);
            compute.SetFloat ("viewRadius", settings.perceptionRadius);
            compute.SetFloat ("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt (numBoids / (float) threadGroupSize);
            compute.Dispatch (0, threadGroups, 1, 1);

            boidBuffer.GetData (boidData);

            // Calculate player distance if in multiplayer mode
            float playerDistance = float.MaxValue;
            if (isMultiplayerMode && player1Target != null && player2Target != null)
            {
                playerDistance = Vector3.Distance(player1Target.position, player2Target.position);
            }

            for (int i = 0; i < boids.Length; i++) {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                // Update boid colors based on player distance and game state
                if (isMultiplayerMode)
                {
                    if (isGameWon)
                    {
                        // Game is won, always use unified flock color regardless of distance
                        boids[i].SetColour(unifiedFlockColor);
                    }
                    else if (playerDistance <= splitDistance)
                    {
                        // Players are close, all boids same color
                        boids[i].SetColour(unifiedFlockColor);
                    }
                    else
                    {
                        // Players are far apart, color based on assignment
                        boids[i].SetColour(boids[i].playerAssignment == 1 ? player1Color : player2Color);
                    }
                }

                boids[i].UpdateBoid ();
            }

            boidBuffer.Release ();
        }
    }

    public struct BoidData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size {
            get {
                return sizeof (float) * 3 * 5 + sizeof (int);
            }
        }
    }
}