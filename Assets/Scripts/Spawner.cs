using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never, SelectedOnly, Always }

    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color colour;
    public GizmoType showSpawnRegion;

    void Awake () {
        // Wait a frame to ensure PlayerManager has registered all players
        StartCoroutine(SpawnBoidsAfterDelay());
    }
    
    private IEnumerator SpawnBoidsAfterDelay() {
        // Wait for a frame to ensure all players are registered
        yield return null;
        
        // Get the PlayerManager
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        int boidsToSpawn = spawnCount;
        
        // If PlayerManager exists, use its boid count calculation
        if (playerManager != null) {
            boidsToSpawn = playerManager.GetTotalBoidCount();
            Debug.Log("Spawning " + boidsToSpawn + " boids based on player count");
        }
        
        // Spawn the boids
        for (int i = 0; i < boidsToSpawn; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate (prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;

            boid.SetColour (colour);
        }
    }

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {

        Gizmos.color = new Color (colour.r, colour.g, colour.b, 0.3f);
        Gizmos.DrawSphere (transform.position, spawnRadius);
    }

}