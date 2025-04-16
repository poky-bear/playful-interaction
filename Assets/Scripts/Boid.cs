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

    void Awake () {
        material = transform.GetComponentInChildren<MeshRenderer> ().material;
        cachedTransform = transform;
    }

    public void Initialize (BoidSettings settings, Transform target) {
        this.target = target;
        this.settings = settings;

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

        // Get the BoidManager to check multiplayer status
        BoidManager manager = FindObjectOfType<BoidManager>();
        Transform currentTarget = target; // Default to legacy target

        if (manager != null) {
            // Check if we're in multiplayer mode with valid targets
            if (manager.player1Target != null && manager.player2Target != null) {
                float distance = Vector3.Distance(manager.player1Target.position, manager.player2Target.position);
                
                if (distance <= 5f) { // Use the same splitDistance as BoidManager
                    // Players are close, use midpoint as target
                    Vector3 midpoint = (manager.player1Target.position + manager.player2Target.position) / 2f;
                    Vector3 offsetToTarget = (midpoint - position);
                    acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
                } else {
                    // Players are far apart, follow assigned player
                    Transform assignedTarget = (playerAssignment == 1) ? manager.player1Target : manager.player2Target;
                    if (assignedTarget != null && assignedTarget.gameObject.activeInHierarchy) {
                        Vector3 offsetToTarget = (assignedTarget.position - position);
                        acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
                    }
                }
            } else {
                // Legacy single-player targeting
                if (target != null && target.gameObject.activeInHierarchy) {
                    Vector3 offsetToTarget = (target.position - position);
                    acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
                }
            }
        }

        if (numPerceivedFlockmates != 0) {
            // Calculate center based on target if available, otherwise use flockmate average
            Vector3 center = (target != null) ? target.position : (centreOfFlockmates / numPerceivedFlockmates);
            Vector3 offsetToCenter = (center - position);

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToCenter) * settings.cohesionWeight;
            var seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp (speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
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

    Vector3 SteerTowards (Vector3 vector) {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

}