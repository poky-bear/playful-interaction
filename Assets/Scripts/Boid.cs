using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    BoidSettings settings;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;
    
    // Multiplayer support
    [HideInInspector]
    public int playerAssignment = 0; // 0 = unassigned, 1 = player1, 2 = player2
    private BoidManager boidManager; // Reference to the manager for multiplayer info

    void Awake () {
        material = transform.GetComponentInChildren<MeshRenderer> ().material;
        cachedTransform = transform;
    }

    public void Initialize (BoidSettings settings, Transform target) {
        this.target = target;
        this.settings = settings;

        // Get reference to BoidManager
        boidManager = FindObjectOfType<BoidManager>();
        if (boidManager == null) {
            Debug.LogWarning("No BoidManager found in scene. Multiplayer features will be disabled.");
        }

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    public void SetColour (Color col) {
        if (material != null) {
            material.color = col;
        }
    }

    public void UpdateBoid () {
        Vector3 acceleration = Vector3.zero;

        Transform currentTarget = target;

        // Handle multiplayer targeting if BoidManager is available
        if (boidManager != null && boidManager.player1Target != null && boidManager.player2Target != null) {
            float distance = Vector3.Distance(boidManager.player1Target.position, boidManager.player2Target.position);
            
            // Check if the game is won - if so, always follow the center when players are close
            if (boidManager.isGameWon) {
                // When game is won, always use midpoint as target when players are close
                if (distance <= boidManager.splitDistance) {
                    Vector3 midpoint = (boidManager.player1Target.position + boidManager.player2Target.position) / 2f;
                    Vector3 offsetToTarget = (midpoint - position);
                    acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
                } else {
                    // Even when game is won, if players are far apart, follow assigned player
                    Transform assignedTarget = (playerAssignment == 1) ? boidManager.player1Target : boidManager.player2Target;
                    if (assignedTarget != null && assignedTarget.gameObject.activeInHierarchy) {
                        Vector3 offsetToTarget = (assignedTarget.position - position);
                        acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
                    }
                }
            } else {
                // Normal multiplayer behavior (game not won yet)
                if (distance <= boidManager.splitDistance) {
                    // Players are close, use midpoint as target
                    Vector3 midpoint = (boidManager.player1Target.position + boidManager.player2Target.position) / 2f;
                    Vector3 offsetToTarget = (midpoint - position);
                    acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
                } else {
                    // Players are far apart, follow assigned player
                    Transform assignedTarget = (playerAssignment == 1) ? boidManager.player1Target : boidManager.player2Target;
                    if (assignedTarget != null && assignedTarget.gameObject.activeInHierarchy) {
                        Vector3 offsetToTarget = (assignedTarget.position - position);
                        acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
                    }
                }
            }
        } else {
            // Legacy single-player targeting
            if (target != null && target.gameObject.activeInHierarchy) {
                Vector3 offsetToTarget = (target.position - position);
                acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
            }
        }

        if (numPerceivedFlockmates != 0) {
            // Calculate center based on flockmate average
            Vector3 center = centreOfFlockmates / numPerceivedFlockmates;
            Vector3 offsetToCenter = (center - position);

            // Only apply forces if the vectors are valid (not NaN)
            if (!float.IsNaN(avgFlockHeading.x) && !float.IsNaN(offsetToCenter.x) && !float.IsNaN(avgAvoidanceHeading.x)) {
                var alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
                var cohesionForce = SteerTowards(offsetToCenter) * settings.cohesionWeight;
                var seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += seperationForce;
            } else {
                Debug.LogWarning($"[Boid] Invalid force vectors detected. ID: {GetInstanceID()}, Pos: {position}");
            }
        }

        if (IsHeadingForCollision ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        // Check for invalid acceleration
        if (float.IsNaN(acceleration.x) || float.IsNaN(acceleration.y) || float.IsNaN(acceleration.z)) {
            Debug.LogWarning($"[Boid] Invalid acceleration detected. ID: {GetInstanceID()}, Pos: {position}");
            return;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        
        // Prevent division by zero
        if (speed < 0.0001f) {
            Debug.LogWarning($"[Boid] Near-zero velocity detected. ID: {GetInstanceID()}, Pos: {position}");
            return;
        }

        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        // Final validation before applying movement
        if (!float.IsNaN(velocity.x) && !float.IsNaN(velocity.y) && !float.IsNaN(velocity.z)) {
            Vector3 newPosition = cachedTransform.position + velocity * Time.deltaTime;
            
            // Only update if the new position is valid
            if (!float.IsNaN(newPosition.x) && !float.IsNaN(newPosition.y) && !float.IsNaN(newPosition.z)) {
                cachedTransform.position = newPosition;
                cachedTransform.forward = dir;
                position = cachedTransform.position;
                forward = dir;
            } else {
                Debug.LogWarning($"[Boid] Invalid position calculated. ID: {GetInstanceID()}, Current: {position}, Velocity: {velocity}");
            }
        } else {
            Debug.LogWarning($"[Boid] Invalid velocity calculated. ID: {GetInstanceID()}, Pos: {position}, Acc: {acceleration}");
        }
    }

    bool IsHeadingForCollision () {
        RaycastHit hit;
        if (Physics.SphereCast (position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask)) {
            return true;
        } else { }
        return false;
    }

    Vector3 ObstacleRays () {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = cachedTransform.TransformDirection (rayDirections[i]);
            Ray ray = new Ray (position, dir);
            if (!Physics.SphereCast (ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask)) {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards(Vector3 vector) {
        // Check for zero vector
        if (vector.sqrMagnitude < 0.000001f) {
            return Vector3.zero;
        }

        // Normalize and calculate steering force
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        
        // Validate result
        if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)) {
            Debug.LogWarning($"[Boid] Invalid steering force calculated. ID: {GetInstanceID()}, Input: {vector}, Velocity: {velocity}");
            return Vector3.zero;
        }

        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

}