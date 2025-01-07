using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D }
    public Type enemyType;
    public int maxHealth;
    public int currentHealth;
    public int point;
    //public Transform target;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public List<Transform> waypoints = new List<Transform>();
    private Transform targetWaypoint;
    private int targetWaypointIndex = 0;
    private float minDistance = 0.1f;
    private int lastWaypointIndex;

    public float movementSpeed = 5.0f;
    private float rotationSpeed = 2.0f;

    private void Start()
    {
        currentHealth = maxHealth;

        lastWaypointIndex = waypoints.Count - 1;
        // Move to the first waypoint when the enemy spawn
        targetWaypoint = waypoints[targetWaypointIndex];
    }

    private void Update()
    {
        float movementStep = movementSpeed * Time.deltaTime;
        float rotationStep = rotationSpeed * Time.deltaTime;

        Vector3 directionToTarget = targetWaypoint.position - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotationToTarget, rotationStep);

        Debug.DrawRay(transform.position, transform.forward * 50f, Color.green, 0f);
        Debug.DrawRay(transform.position, directionToTarget, Color.red, 0f);


        float distance=Vector3.Distance(transform.position, targetWaypoint.position);
        CheckDistanceToWaypoint(distance);

        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, movementStep);

        
    }

    private void CheckDistanceToWaypoint(float currentDistance)
    {
        if(currentDistance <= minDistance)
        {
            targetWaypointIndex++;
            UpdateTargetWaypoint();
        }
    }

    private void UpdateTargetWaypoint()
    {
        // This code should be changed(When the enemy reached to the last waypoint(Base), it should start attack the base)
        if (targetWaypointIndex > lastWaypointIndex)
        {
            targetWaypointIndex = 0;
        }

        targetWaypoint = waypoints[targetWaypointIndex];
    }

    public void TakeDamage(int amount, bool isGrenade)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void HitByGrenade(Vector3 explosionPosition)
    {
        currentHealth -= 100;
        Vector3 reactVec = transform.position = explosionPosition;
        TakeDamage(currentHealth, true);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
